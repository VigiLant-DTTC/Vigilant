/**
 * ARQUIVO: GenericPartial.js
 * Lógica Genérica para o carregamento de Partial Views via AJAX na modal.
 * * Depende dos botões terem os atributos:
 * - data-title (título da modal)
 * - data-url (URL da Action do Controller) - USADO APENAS PARA CREATE
 * - data-controller-url (URL base para ações de tabela)
 * - data-action-type (Tipo de ação: 'edit', 'details', 'delete' ou 'create-modal')
 * - data-id (ID do registro para ações de tabela)
 * - data-form-id (ID do formulário dentro da Partial View)
 */

// 1. Definição das variáveis globais dos elementos da modal
const modalOverlay = document.getElementById('ajax-modal');
const modalTitle = document.getElementById('modalTitle');
const modalBody = document.getElementById('modalBody');
const closeModalButton = document.getElementById('closeModalButton');


// --- FUNÇÕES DE CONTROLE DA MODAL ---

function openModal(title) {
    modalTitle.textContent = title;
    modalOverlay.classList.add('show');
    document.body.style.overflow = 'hidden';
}

function closeModal() {
    modalOverlay.classList.remove('show');
    // Limpa o corpo da modal após a animação (300ms)
    setTimeout(() => {
        modalBody.innerHTML = '';
        document.body.style.overflow = '';
    }, 300);
}

function loadPartialView(url, method, title, formId) {
    // 1. Mostrar loading e abrir a modal com o título correto
    modalBody.innerHTML = '<div style="text-align: center; padding: 50px;">Carregando...</div>';
    openModal(title);

    // 2. Requisição AJAX
    fetch(url, {
        method: method,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            // Trata erro de Validação (400) no Controller, permitindo que a Partial View seja lida
            if (!response.ok && response.status !== 400) {
                throw new Error(`Erro ${response.status}: ${response.statusText}`);
            }
            return response.text();
        })
        .then(html => {
            modalBody.innerHTML = html;

            // 3. Re-bind dos listeners e validação, passando o ID do formulário
            bindModalEventListeners(formId);
        })
        .catch(error => {
            console.error('Erro ao carregar Partial View:', error);
            modalBody.innerHTML = `<div class="delete-message">Ocorreu um erro ao carregar o conteúdo: ${error.message}</div>`;
            modalTitle.textContent = 'Erro de Carregamento';
        });
}


// --- LÓGICA DE SUBMISSÃO DE FORMULÁRIOS (AJAX) ---

// Função agora recebe o ID do formulário para ser genérica
function handleFormSubmission(formId) {
    const form = document.getElementById(formId);
    if (!form) return;

    // A CORREÇÃO PRINCIPAL ESTÁ AQUI:
    // Remove o handler antigo antes de adicionar um novo.
    // O handler deve ser armazenado diretamente no elemento do formulário.
    if (form.currentSubmitHandler) {
        form.removeEventListener('submit', form.currentSubmitHandler);
    }

    // Define o novo handler.
    form.currentSubmitHandler = function (e) {
        e.preventDefault();

        // 1. Validação do lado do cliente (jQuery Unobtrusive)
        if (window.jQuery && !jQuery(form).valid()) {
            return;
        }

        const formData = new FormData(form);
        const actionUrl = form.getAttribute('action');
        const method = form.getAttribute('method') || 'POST';

        // Desabilitar o botão de submit temporariamente para evitar cliques múltiplos
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processando...';
        }


        fetch(actionUrl, {
            method: method,
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                // Reabilitar o botão de submit em caso de resposta (sucesso ou erro)
                if (submitButton) {
                    // Restaurar o conteúdo original do botão - é necessário um atributo data-original-html
                    // mas para simplificar, vamos apenas reabilitar
                    submitButton.disabled = false;
                    // Assumindo que o HTML original está definido no _ConectarEquipamentosPartial.cshtml,
                    // vamos apenas reabilitar o botão aqui, o re-bind 400 corrigirá o HTML.
                }

                const isAIAnalysis = actionUrl.includes('ProcessarAnalise');

                if (response.ok) {
                    if (isAIAnalysis) {
                        // FLUXO DE IA (Substitui o formulário pelo resultado)
                        return response.text().then(html => {
                            modalBody.innerHTML = html;
                            // O re-bind é CRUCIAL: Adiciona o listener para o botão 'Fechar'
                            // Embora não haja um formId para a tela de resultado, 
                            // bindModalEventListeners cuidará do botão de Cancelar/Fechar.
                            bindModalEventListeners('iaAnalysisForm'); // Passa o ID do form anterior
                        });
                    } else {
                        // FLUXO CRUD PADRÃO (Successo na Criação/Edição/Exclusão)
                        closeModal();
                        // Recarrega a página para atualizar a tabela
                        window.location.reload();
                    }
                } else if (response.status === 400) {
                    // Erro de Validação (Controller retorna PartialView com ModelState.Errors)
                    return response.text().then(html => {
                        modalBody.innerHTML = html;
                        // O re-bind é CRUCIAL após o erro 400. Ele re-analisa o form e re-adiciona o submit handler.
                        bindModalEventListeners(formId);
                        // Removido o alert, pois a validação deve ser exibida no formulário
                    });
                } else {
                    // Outro erro (500, etc.)
                    throw new Error(`Erro ${response.status}: ${response.statusText}`);
                }
            })
            .catch(error => {
                console.error('Erro na submissão do formulário:', error);
                alert(`Erro na submissão: ${error.message}`);
                // Reabilitar o botão de submit em caso de erro na requisição (ex: falha de rede)
                if (submitButton) {
                    submitButton.disabled = false;
                    // Para restaurar o ícone original, seria necessário ter salvo o innerHTML original
                    // Exemplo: submitButton.innerHTML = submitButton.getAttribute('data-original-html');
                }
            });
    };

    // Adiciona o novo handler.
    form.addEventListener('submit', form.currentSubmitHandler);
}


// --- EVENT LISTENERS GERAIS (INÍCIO DO FLUXO) ---

// 1. Lógica para Botões de CRIAR (data-action-type="create-modal")
document.querySelectorAll('[data-action-type="create-modal"]').forEach(button => {
    button.addEventListener('click', function () {
        const url = this.getAttribute('data-url');
        const title = this.getAttribute('data-title');
        const formId = this.getAttribute('data-form-id');
        loadPartialView(url, 'GET', title, formId);
    });
});

document.querySelectorAll('[data-action-type="aceitar-solicitacao-modal"]').forEach(button => {
    button.addEventListener('click', function () {
        const solicitacaoId = this.getAttribute('data-id');
        const baseControllerUrl = this.getAttribute('data-controller-url');
        const formId = this.getAttribute('data-form-id');
        const title = this.getAttribute('data-title');

        // URL aponta para o novo método no Controller de Notificações
        const url = `${baseControllerUrl}/AbrirFormularioAceitarSolicitacao/${solicitacaoId}`;

        loadPartialView(url, 'GET', title, formId);
    });
});

// 2. Lógica para Botões de Ação na Tabela (EDITAR/DETALHES/EXCLUIR)
// Selecionamos todos os botões com a classe de ação da modal para evitar problemas de delegação.
document.querySelectorAll('.btn-ajax-modal').forEach(button => {
    button.addEventListener('click', function () {
        const id = this.getAttribute('data-id');
        const actionType = this.getAttribute('data-action-type');
        const baseControllerUrl = this.getAttribute('data-controller-url');
        const formId = this.getAttribute('data-form-id');
        const titleTemplate = this.getAttribute('data-title');

        let url = '';
        let title = titleTemplate.replace('{ID}', id);

        // Constrói a URL dinamicamente
        if (actionType === 'delete') {
            // Ação de Exclusão usa a Action "DeleteConfirmation" no Controller
            url = `${baseControllerUrl}/DeleteConfirmation/${id}`;
        } else if (actionType === 'create-from-solicitacao') {
             // Chama Colaboradores/Create com o ID da Solicitação como parâmetro de query string
             url = `${baseControllerUrl}/Create?solicitacaoId=${id}`;
             title = titleTemplate;
        }
        else {
            // Ações "Edit" e "Details" usam a Action com o mesmo nome
            url = `${baseControllerUrl}/${actionType}/${id}`;
        }

        loadPartialView(url, 'GET', title, formId);
    });
});


// 3. Fechar a modal pelo botão 'X' no header
closeModalButton.addEventListener('click', closeModal);

// 4. Fechar a modal clicando fora
modalOverlay.addEventListener('click', function (e) {
    if (e.target === modalOverlay) {
        closeModal();
    }
});


// --- EVENT LISTENERS INTERNOS (RE-BIND APÓS CARREGAR PARTIAL) ---

function bindModalEventListeners(formId) {
        const cancelButton = document.getElementById('cancelModalButton'); // Usar ID genérico
        if (cancelButton) {
            // Remove e adiciona o listener para garantir que só haja um
            cancelButton.removeEventListener('click', closeModal);
            cancelButton.addEventListener('click', closeModal);
        }

        // 2. Reativação da Validação Unobtrusive (Crucial!)
        // Apenas aplica no formulário carregado.
        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
            const form = document.getElementById(formId);
            if (form) {
                // Remove dados de validação antigos e re-analisa o formulário
                jQuery(form).removeData('validator');
                jQuery(form).removeData('unobtrusiveValidation');
                jQuery.validator.unobtrusive.parse(form);

                jQuery(form).off('submit').on('submit', function (e) {
                    e.preventDefault();
                    const $form = jQuery(this);

                    // Verificação de validação do lado do cliente
                    if (!$form.valid()) {
                        return;
                    }

                    const originalButtonContent = $form.find('button[type="submit"]').html();
                    $form.find('button[type="submit"]').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processando...');

                    jQuery.ajax({
                        url: $form.attr('action'),
                        type: 'POST',
                        data: $form.serialize(),
                        headers: {
                            "X-Requested-With": "XMLHttpRequest"
                        },
                        success: function (response) {
                            if (response.success && response.analiseId) {
                                // Se a análise foi gerada (caso do GerarAnalise)
                                alert(response.message || 'Operação realizada com sucesso!');
                                closeModal();
                                // Redirecionar para os detalhes da análise recém-criada
                                window.location.href = '/Analises/Details/' + response.analiseId;
                            } else {
                                // Se for um CREATE ou EDIT (que retornaria a partial view com o formulário atualizado ou vazio)
                                // ou se houver um erro de validação (tratado abaixo)

                                // Recarrega a página se o sucesso for genérico (após um DELETE ou CREATE/EDIT bem-sucedido)
                                alert(response.message || 'Operação realizada com sucesso!');
                                closeModal();
                                window.location.reload();
                            }
                        },
                        error: function (xhr) {
                            // Se o servidor retornar uma Partial View (geralmente em caso de erro de validação no CREATE/EDIT)
                            if (xhr.getResponseHeader('Content-Type')?.includes('text/html')) {
                                // Substitui o conteúdo do modal pelo novo HTML (a partial view com erros)
                                modalBody.innerHTML = xhr.responseText;
                                // Re-bind nos listeners da nova partial view
                                bindModalEventListeners(formId);
                            } else {
                                // Erro genérico
                                alert('Erro: ' + (xhr.responseJSON?.message || xhr.responseText || 'Ocorreu um erro desconhecido.'));
                                closeModal();
                            }
                        },
                        complete: function () {
                            // Resetar o botão de submit (se não houve sucesso e redirecionamento)
                            $form.find('button[type="submit"]').prop('disabled', false).html(originalButtonContent);
                        }
                    });
                });
            }
        }

        // 3. Submissão de Formulário (aplica o handler APENAS no formulário carregado)
        handleFormSubmission(formId);
    }