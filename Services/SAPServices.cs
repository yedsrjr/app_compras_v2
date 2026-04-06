﻿using AppCompras.Models.Data;
using AppCompras.Models.ViewModels;
using System.Text.Json;

namespace AppCompras.Services
{
    public class SAPServices(SAPRepository services, LogService logService)
    {
        public async Task<ServiceResult<List<ItemCompraViewModel>>> BuscarPorPedidoCompra(
            string codPedido,
            int userId,
            string username,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            if (!int.TryParse(codPedido, out _))
                return ServiceResult<List<ItemCompraViewModel>>.Fail("O código do pedido deve ser numérico");

            await logService.RecordLog(userId, username, "Consulta por Pedido de Compra", $"DocNum: {codPedido}");
            return await ExecutarBusca(GetBaseQuery(dataInicio, dataFim) + $" WHERE o.DocNum = {codPedido} ORDER BY p.LineNum ASC");
        }

        public async Task<ServiceResult<List<ItemCompraViewModel>>> BuscarPorItem(
            string codigoItem,
            int userId,
            string username,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            // Sanitização básica para evitar injeção de SQL em strings
            var sanitizedItem = codigoItem.Replace("'", "''");

            await logService.RecordLog(userId, username, "Consulta por Item", $"ItemCode: {sanitizedItem}");
            return await ExecutarBusca(GetBaseQuery(dataInicio, dataFim) + $" WHERE p.ItemCode = '{sanitizedItem}' ORDER BY p.LineNum ASC");
        }

        public async Task<ServiceResult<List<ItemCompraViewModel>>> BuscarPorDescricao(
            string descricao,
            int userId,
            string username,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            // Sanitização para evitar injeção de SQL
            var sanitizedDesc = descricao.Replace("'", "''");

            await logService.RecordLog(userId, username, "Consulta por Descrição", $"Termo: {sanitizedDesc}");
            // Utiliza LIKE para busca flexível por descrição
            return await ExecutarBusca(GetBaseQuery(dataInicio, dataFim) + $" WHERE p.Dscription LIKE '%{sanitizedDesc}%' ORDER BY p.LineNum ASC");
        }

        /// <summary>
        /// Valida se uma solicitação pode ser alterada com base na data e no perfil do usuário (RF05)
        /// </summary>
        public ServiceResult<bool> ValidarPermissaoAlteracao(DateTime dataSolicitacao, UserRole userRole)
        {
            var diasDiferenca = (DateTime.Now - dataSolicitacao).TotalDays;

            // Se passou de 180 dias e não for Gerente ou Admin, bloqueia
            if (diasDiferenca > 180 && userRole == UserRole.Comprador)
            {
                return ServiceResult<bool>.Fail("Alterações em solicitações com mais de 180 dias exigem autorização de um Gerente.");
            }

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<List<MenorValorViewModel>>> BuscarMenorValor(
            string tipoBusca,
            string valorBusca,
            int userId,
            string username,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            if (string.Equals(tipoBusca, "pedido", StringComparison.OrdinalIgnoreCase) && !int.TryParse(valorBusca, out _))
            {
                return ServiceResult<List<MenorValorViewModel>>
                    .Fail("O código do pedido deve ser numérico");
            }

            var filtroPeriodo = BuildDateFilter("pc.DocDate", dataInicio, dataFim, "DATEADD(DAY, -90, GETDATE())");
            var (whereClause, logText) = BuildMenorValorWhere(tipoBusca, valorBusca);

            if (whereClause == null)
            {
                return ServiceResult<List<MenorValorViewModel>>
                    .Fail("Tipo de busca inválido.");
            }

            await logService.RecordLog(userId, username, "Consulta Menor Valor", logText);

            var query = $@"
                SELECT
                    pc.DocNum [Cod_PC],
                    CONCAT(pch.ItemCode, ' - ', i.ItemName ) descricao,
                    CONCAT(pc.CardCode, ' - ', o.CardName) mv_fornecedor,
                    CAST(pc.DocDate as DATE) data_pc,
                    pch.Quantity qtd_pc,
                    pch.Price preco_unit,
                    (pch.Price * pch.Quantity) valor_total_item
                FROM [OPOR] pc
                LEFT JOIN [POR1] pch ON pc.DocNum = pch.DocEntry
                LEFT JOIN [OITM] i ON i.ItemCode = pch.ItemCode 
                LEFT JOIN [OCRD] o ON o.CardCode = pc.CardCode
                WHERE {whereClause} AND pc.DocStatus = 'C' AND {filtroPeriodo}
                ORDER BY pch.Price ASC";

            return await ExecutarBuscaMenorValor(query);
        }

        private string GetBaseQuery(DateTime? dataInicio, DateTime? dataFim)
        {
            var filtroPeriodo = BuildDateFilter("o.DocDate", dataInicio, dataFim, "DATEADD(DAY, -90, GETDATE())");
            return @"
                SELECT 
                    o.DocNum as [Cod.], 
                    CONCAT(p.ItemCode, ' - ', p.Dscription) descricao, 
                    p.LineNum, 
                    mv.DocEntry mv_cod_pc, 
                    mv.Price mv_preco, 
                    mv.mv_fornecedor, 
                    mv.Quantity mv_qtd,
                    mv.date mv_data, 
                    uv.DocEntry uv_cod_pc, 
                    uv.Price uv_preco, 
                    uv.uv_fornecedor, 
                    uv.Data uv_data, 
                    uv.Quantity uv_qtd, 
                    mp.media media_ponderada
                FROM OPOR o 
                LEFT JOIN POR1 p ON o.DocEntry = p.DocEntry
                LEFT JOIN (
                    SELECT * FROM (
                        SELECT o.DocEntry, 
                            p.ItemCode,  
                            p.Quantity, 
                            p.Price,
                            CONCAT(o.CardCode, ' - ', o2.CardName) mv_fornecedor,
                            ROW_NUMBER() OVER(PARTITION BY p.ItemCode ORDER BY p.Price ASC) rd,
                            CONVERT(DATE, o.DocDate, 23) as date
                        FROM OPOR o 
                        LEFT JOIN POR1 p ON o.DocEntry = p.DocEntry
                        LEFT JOIN OCRD o2 ON o.CardCode = o2.CardCode
                        WHERE " + filtroPeriodo + @"
                    ) as t0
                    WHERE t0.rd = 1
                ) as mv ON p.ItemCode = mv.ItemCode
                LEFT JOIN (
                    SELECT * FROM (
                        SELECT CONVERT(DATE, o.DocDate, 23) as data,
                            o.DocEntry,
                            p.ItemCode,  
                            p.Quantity, 
                            p.Price,
                            CONCAT(o.CardCode, ' - ', o3.CardName) uv_fornecedor,
                            ROW_NUMBER() OVER(PARTITION BY p.ItemCode ORDER BY p.DocDate DESC, p.Price ASC) rd 
                        FROM OPOR o 
                        LEFT JOIN POR1 p ON o.DocEntry = p.DocEntry
                        LEFT JOIN OCRD o3 ON o.CardCode = o3.CardCode
                    ) as t1
                    WHERE t1.rd = 1
                ) uv ON p.ItemCode = uv.ItemCode
                LEFT JOIN (
                    SELECT p.ItemCode,
                        SUM(p.Quantity*p.Price)/SUM(p.Quantity) as media 
                    FROM OPOR o
                    LEFT JOIN POR1 p ON o.DocEntry = p.DocEntry
                    WHERE " + filtroPeriodo + @"
                    GROUP BY p.ItemCode
                ) mp ON p.ItemCode = mp.ItemCode";
        }

        private static (string? WhereClause, string LogText) BuildMenorValorWhere(string tipoBusca, string valorBusca)
        {
            if (string.Equals(tipoBusca, "pedido", StringComparison.OrdinalIgnoreCase))
            {
                return ($"pc.DocNum = {valorBusca}", $"DocNum: {valorBusca}");
            }

            if (string.Equals(tipoBusca, "item", StringComparison.OrdinalIgnoreCase))
            {
                var sanitizedItem = valorBusca.Replace("'", "''");
                return ($"pch.ItemCode = '{sanitizedItem}'", $"ItemCode: {sanitizedItem}");
            }

            if (string.Equals(tipoBusca, "descricao", StringComparison.OrdinalIgnoreCase))
            {
                var sanitizedDesc = valorBusca.Replace("'", "''");
                return ($"i.ItemName LIKE '%{sanitizedDesc}%'", $"Descricao: {sanitizedDesc}");
            }

            return (null, string.Empty);
        }

        private static string BuildDateFilter(string column, DateTime? dataInicio, DateTime? dataFim, string defaultStartExpression)
        {
            if (dataInicio.HasValue && dataFim.HasValue)
            {
                return $"{column} >= '{dataInicio:yyyy-MM-dd}' AND {column} <= '{dataFim:yyyy-MM-dd}'";
            }

            if (dataInicio.HasValue)
            {
                return $"{column} >= '{dataInicio:yyyy-MM-dd}'";
            }

            if (dataFim.HasValue)
            {
                return $"{column} <= '{dataFim:yyyy-MM-dd}'";
            }

            return $"{column} >= {defaultStartExpression}";
        }

        private async Task<ServiceResult<List<ItemCompraViewModel>>> ExecutarBusca(string query)
        {
            try
            {
                var response = await services.BuscarSapQuery(query);

                if (string.IsNullOrEmpty(response))
                {
                    return ServiceResult<List<ItemCompraViewModel>>
                        .Fail("Nenhum dado retornado pela API");
                }

                var dados = JsonSerializer.Deserialize<List<ItemCompraViewModel>>(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (dados == null || dados.Count == 0)
                {
                    return ServiceResult<List<ItemCompraViewModel>>
                        .Ok(new List<ItemCompraViewModel>(), "Nenhum resultado encontrado");
                }

                return ServiceResult<List<ItemCompraViewModel>>
                    .Ok(dados, $"{dados.Count} item(ns) encontrado(s)");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"ERRO JSON: {jsonEx.Message}");
                return ServiceResult<List<ItemCompraViewModel>>
                    .Fail($"Erro ao processar resposta: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                return ServiceResult<List<ItemCompraViewModel>>
                    .Fail($"Erro ao buscar dados: {ex.Message}");
            }
        }

        private async Task<ServiceResult<List<MenorValorViewModel>>> ExecutarBuscaMenorValor(string query)
        {
            try
            {
                var response = await services.BuscarSapQuery(query);

                if (string.IsNullOrEmpty(response))
                {
                    return ServiceResult<List<MenorValorViewModel>>
                        .Fail("Nenhum dado retornado pela API");
                }

                var dados = JsonSerializer.Deserialize<List<MenorValorViewModel>>(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (dados == null || dados.Count == 0)
                {
                    return ServiceResult<List<MenorValorViewModel>>
                        .Ok(new List<MenorValorViewModel>(), "Nenhum resultado encontrado");
                }

                return ServiceResult<List<MenorValorViewModel>>
                    .Ok(dados, $"{dados.Count} item(ns) encontrado(s)");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"ERRO JSON: {jsonEx.Message}");
                return ServiceResult<List<MenorValorViewModel>>
                    .Fail($"Erro ao processar resposta: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                return ServiceResult<List<MenorValorViewModel>>
                    .Fail($"Erro ao buscar dados: {ex.Message}");
            }
        }

    }
}
