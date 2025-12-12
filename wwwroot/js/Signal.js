<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js"></script>

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/medicaoHub") // O endpoint que você configurou no Startup.cs (ou Program.cs)
    .withAutomaticReconnect() // Tenta reconectar automaticamente
    .build();

// 2. Listener para o evento de Atualização do Equipamento (Disparado pelo MqttClientService.cs)
connection.on("ReceberAtualizacaoEquipamento", (equipamento) => {
    const $row = $(`#equipamento-${equipamento.id}`);

    if ($row.length) {
        // 1. Atualiza os dados na tabela de listagem

        // Atualiza a Última Medição na célula que tem a classe 'medicao-dinamica'
        const $medicaoCell = $row.find('td.medicao-dinamica');
        $medicaoCell.text(equipamento.valorMedicao); // <-- CRÍTICO: Usa o ValorMedicao do SignalR

        // Atualiza o Status Badge
        const $statusBadge = $row.find('.status-badge');
        // O status vem como string ("Conectado", "Desconectado", etc.) ou número (Enum)
        // Se o status vier como string, compare a string. Se vier como int, compare o int.
        // Usando a lógica do backend:
        let statusClass = 'info';
        if (equipamento.status === 1 || equipamento.status === 'Conectado') statusClass = 'success';
        else if (equipamento.status === 2 || equipamento.status === 'Desconectado') statusClass = 'danger';
        else if (equipamento.status === 3 || equipamento.status === 'Erro') statusClass = 'warning';

        // Remove todas as classes de status e adiciona a correta
        $statusBadge.removeClass().addClass('status-badge ' + statusClass).text(equipamento.status);

        // Atualiza a Última Atualização (Ajuste o índice se a tabela mudou)
        // Assumindo que a coluna 'Última Atualização' é a 7ª coluna (índice 6), após a 'Última Medição' (índice 5)
        // ID(0), Nome(1), Localização(2), Tipo(3), Status(4), Medição(5), Atualização(6)
        $row.find('td:eq(6)').text(equipamento.ultimaAtualizacao);

        // Opcional: Atualiza Nome e Localização (Caso o sensor mande uma atualização de registro)
        $row.find('td:eq(1)').text(equipamento.nome); // Nome
        $row.find('td:eq(2)').text(equipamento.localizacao); // Localização

        console.log(`Equipamento #${equipamento.id} atualizado: ${equipamento.valorMedicao}`);
    }

    // 3. Opcional: Atualiza a Modal de Monitoramento (se estiver aberta)
    // Isso garante que, se o usuário estiver na modal, o dado seja atualizado em tempo real.
    const $monitorModal = $('#ajax-modal');
    if ($monitorModal.is(':visible')) {
        const modalEquipamentoId = $monitorModal.data('equipamento-id');

        if (modalEquipamentoId == equipamento.id) {
            $('#monitor-valor-medicao').text(equipamento.valorMedicao);
            $('#monitor-nome').text(equipamento.nome);
            // Você pode adicionar mais atualizações de status/localização aqui se a modal tiver os IDs
        }
    }
});

// 4. Inicia a conexão
async function startSignalR() {
    try {
        await connection.start();
        console.log("Conexão SignalR estabelecida com sucesso.");
    } catch (err) {
        console.error("Erro ao iniciar conexão SignalR:", err);
        // Tenta reconectar a cada 5 segundos se a conexão inicial falhar
        setTimeout(startSignalR, 5000);
    }
};

startSignalR();