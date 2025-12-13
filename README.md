# 🛡️ VIGILANT
SISTEMA INTELIGENTE DE GESTÃO DE RISCOS 

O projeto VIGILANT é uma solução de software desenvolvida para modernizar e tornar proativa a gestão de riscos na linha de produção do Complexo Ayrton Senna da Renault, no Paraná (Brasil). A plataforma utiliza a Inteligência Artificial (IA) para transformar processos manuais e burocráticos em um sistema digital, preditivo e intuitivo.

<img width="1570" height="732" alt="Tela1" src="https://github.com/user-attachments/assets/1f6a4f35-199a-4c14-a54b-67908a1e04d7" />

---

## Indice

* [Inicio Rápido](#Inicio_Rápido)
* [Problema](#Problema )
* [Design & Extensibilidade](#Design_&_Extensibilidade)
* [Guia de configuração](#GuiaConfig)
* [Testes de Sistema](#Testes)
* [Recursos-Chave](#Recursos-Chave)
* [Tecnologias Usadas](#Tecnologias)
* [Licença](#Licença)
* [Equipe](#Equipe)

---

## Inicio_Rápido

Clone o repositório e execute o exemplo incluído, e em seguida siga as instruções abaixo:

```bash
https://github.com/sleepyline/Vigilant.git
cd Vigilant
Code .
```

Crie um banco de dados chamado ( vigilant )
Logo apos:

```bash
dotnet ef databse update
dotnet watch run
```

---

Apos isso faça login no sistema com conta adm
```bash
Email: adm@vigilant.com.br
Senha: adm123
```

##  Problema 

A gestão de riscos na Renault era baseada em processos manuais e reativos, resultando em:

    Ineficiência: Lentidão no registro e acompanhamento de riscos.

    Falta de Visibilidade: Dificuldade em avaliar a criticidade de um risco e priorizar ações.

    Impacto na Segurança: Maior incidência de erros e acidentes evitáveis.
    

> **A Solução: Vigilant - Gestão Proativa:** O Vigilant é uma plataforma modular desenhada para ser o centro de controle da segurança industrial, alinhando a Renault aos conceitos da Indústria 4.0.

## Design_&_Extensibilidade

## GuiaConfig
Um guia rapido para você configurar seu sensor -

Lista de sensores disponiveis:

```bash
Temperatura = 1 ,
Pressao = 2,
QualidadeArGas = 3,
Luminosidade = 4,
Movimento = 5,
Distancia = 6,
NivelLiquido = 7,
Vibracao = 8 ,
CorrenteEletrica = 9
```

## Testes
Para testar a conexão do broker sem um sensor e asp, configure o MQTT Broker do sistema ( mqttHost, MqttPort e MqttTopicWildcard)
logo apos use o padrão:

Ex: Vigilant/command/ (para ativar seu sensor)
Ex: Vigilant/data/ (para mandar informações do sensor)

De um nome a seu sensor: Ex: Vigilant/command/SENS_01

```bash
{
  "Identificador": "SENS_01",
  "Nome": "Motor Principal",
  "Localizacao": "Almoxarifado Leste",
  "TipoSensor": 9, 
  "Status": 1,
  "ValorMedicao": "15.5A"
}
```

## Recursos-Chave

Módulo	Descrição	Valor
Análise Preditiva (IA)	O diferencial central. Utiliza IA para analisar dados históricos e prever riscos futuros, sugerindo soluções antes que incidentes ocorram.	Transforma a segurança de reativa para proativa.
Dashboard Centralizado	Painel principal com indicadores de segurança (KPIs) em tempo real, fornecendo visão instantânea do status da produção.	Melhora a tomada de decisão estratégica.
Gestão de Riscos Intuitiva	Interface simplificada para registro de riscos por qualquer colaborador, atribuindo prioridade e status de forma fácil.	Aumenta a adesão e o empoderamento dos colaboradores.
Monitoramento Integrado	Permite o cadastro e monitoramento de equipamentos, com emissão de alertas e notificações automáticas em caso de falhas.	Garante resposta rápida e eficiente.

## Tecnologias
Abaixo tem algumas tecnologias utilizazadas na formaçao ou testes do vigilant:
API - `OpenAI` - Prevenções e soluções.
CONEXÃO `BROKER-MQTTX` - Para conversa com sensores.


## Licença
`Vigilant` ainda não possui licença.

---

## Equipe

Este projeto foi desenvolvido por estudantes desenvolvedores do SENAI, com a participação de:
Nome do Componente	Instituição de Origem
David Lima	- SENAI SEDE, Camaçari, BA
Misla Brito	- SENAI SEDE, Camaçari, BA
Orlando Lucas - SENAI SEDE, Camaçari, BA
Tiago Andrade - SENAI SEDE, Camaçari, BA
Yuri Silva	- SENAI SEDE, Camaçari, BA


