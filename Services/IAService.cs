using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using VigiLant.Contratos; // Novo using para comunicação HTTP

namespace VigiLant.Services
{

    public class IAService : IIAService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model = "gemini-2.5-flash";
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta";

        // Injetamos IHttpClientFactory para gerenciar o HttpClient
        public IAService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            // 1. Configuração da API Key
            _apiKey = configuration["GeminiSettings:ApiKey"]
                      ?? throw new InvalidOperationException("Gemini API Key não configurada. Verifique appsettings.json.");

            // 2. Criação do HttpClient
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<Tuple<string, string>> GerarAnaliseDeRisco(string nomeRisco, string descricaoRisco)
        {
            var prompt = GerarPrompt(nomeRisco, descricaoRisco);

            // Construção do corpo da requisição no formato JSON esperado pelo Gemini
            var requestBody = new
            {
                contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = prompt } } }
                },
                generationConfig = new { temperature = 0.5, maxOutputTokens = 1000 }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // URL completa para a chamada (endpoint de geração de conteúdo)
            var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";

            try
            {
                // 1. Envia a requisição POST
                var response = await _httpClient.PostAsync(url, content);

                // 2. Verifica o status da resposta
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Tuple.Create($"FALHA NA API GEMINI (Status: {(int)response.StatusCode}): {errorContent}", "Nenhuma solução gerada devido à falha na API.");
                }

                // 3. Desserializa a resposta
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                // Tenta extrair o texto principal da resposta (o caminho JSON pode ser complexo)
                var fullResponse = doc.RootElement
                                      .GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text")
                                      .GetString();

                if (string.IsNullOrEmpty(fullResponse))
                {
                    return Tuple.Create("FALHA DE PARSING: Resposta vazia da IA.", "[Resposta bruta não formatada]");
                }

                // 4. Processamento e Separação dos Resultados (A lógica de parsing robusta)
                return ExtrairPrevisaoESolucoes(fullResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = $"FALHA NA API GEMINI: {ex.Message}";
                return Tuple.Create(errorMessage, "Nenhuma solução gerada devido à falha na API.");
            }
        }

        // Funções auxiliares mantidas para clareza
        private string GerarPrompt(string nomeRisco, string descricaoRisco)
        {
            // PROMPT REFINADO: FOCO NA ESTRUTURA, RESUMO E PROIBIÇÃO DE FORMATAÇÃO
            return $@"
            Você é um analista de segurança e risco corporativo. Sua análise deve ser objetiva, **resumida** e **RIGOROSAMENTE ESTRUTURADA**.
            
            Analise o risco: {nomeRisco}.
            Descrição: {descricaoRisco}.

            Sua tarefa é:
            1. PREVISÃO DE RISCOS FUTUROS: Resuma os riscos futuros potenciais e a probabilidade de ocorrência.
            2. SOLUÇÕES SUGERIDAS: Resuma soluções práticas e imediatas para mitigar esses riscos.

            Formate sua resposta **EXCLUSIVAMENTE**, sem numeração (1., 2., a., -) ou bullet points. Use texto simples e separação por espaço.
            
            USE O FORMATO EXATO (SEM TEXTO ADICIONAL, INTRODUÇÃO, OU TÍTULOS COMO 'Soluções Sugeridas (IA):'):
            ---PREVISAO---
            [Aqui vai o texto **resumido** da Previsão. Use frases simples e separação por espaço.]
            ---SOLUCOES---
            [Aqui vai o texto **resumido** das Soluções. Use frases simples e separação por espaço.]
            ";
        }



        private Tuple<string, string> ExtrairPrevisaoESolucoes(string fullResponse)
        {
            // Tags que o modelo insiste em usar, que serão substituídas pela tag correta.
            var badSplitters = new[] {
        "Soluções Sugeridas (IA):",
        "Soluções Sugeridas:",
        "SOLUÇÕES SUGERIDAS:",
        "Solucoes Sugeridas:", // Incluindo variação sem acento
        "Soluções Sugeridas"   // Apenas a frase
    };
            var correctSplitter = "---SOLUCOES---";

            try
            {
                // 1. Divide pela tag PREVISAO
                var splitByPrevisao = fullResponse.Split(new[] { "---PREVISAO---" }, StringSplitOptions.None);
                if (splitByPrevisao.Length < 2)
                    return Tuple.Create($"FALHA DE FORMATO. Tag PREVISAO ausente. Resposta Bruta: {fullResponse.Trim()}", "[Formato não detectado]");

                var contentAfterPrevisao = splitByPrevisao[1]; // Não fazemos Trim ainda.

                // 2. Normalização: Substitui todas as tags ruins pela tag correta.
                // Isso é a defesa mais forte contra a formatação variada do LLM.
                foreach (var splitter in badSplitters)
                {
                    // O StringComparison.OrdinalIgnoreCase permite que ele encontre tanto "Soluções" quanto "soluções".
                    if (contentAfterPrevisao.Contains(splitter, StringComparison.OrdinalIgnoreCase))
                    {
                        // Substitui a primeira ocorrência do título pela tag correta.
                        contentAfterPrevisao = contentAfterPrevisao.Replace(splitter, correctSplitter, StringComparison.OrdinalIgnoreCase);
                        break; // Sai do loop após a primeira substituição bem-sucedida.
                    }
                }

                // 3. Split Final pela tag correta e agora normalizada.
                var splitBySolucoes = contentAfterPrevisao.Split(new[] { correctSplitter }, StringSplitOptions.None);

                if (splitBySolucoes.Length < 2)
                {
                    return Tuple.Create($"FALHA DE FORMATO. Nenhuma tag de Solução detectada (mesmo após normalização). Resposta Bruta: {fullResponse.Trim()}", "[Formato não detectado]");
                }

                // 4. Trim e Retorno
                var previsao = splitBySolucoes[0].Trim();
                var solucoes = splitBySolucoes[1].Trim();

                return Tuple.Create(previsao, solucoes);
            }
            catch (Exception ex)
            {
                var previsao = $"[ERRO DE PARSING ({ex.Message}). Resposta Bruta da IA]: {fullResponse.Trim()}";
                var solucoes = "[Resposta bruta não formatada]";
                return Tuple.Create(previsao, solucoes);
            }
        }
    }
}