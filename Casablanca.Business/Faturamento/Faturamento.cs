using Benner.Tecnologia.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Casablanca.DataContracts.Faturamento;
using Benner.Tecnologia.Common.Instrumentation;
using Benner.Tecnologia.Common;
using Casablanca.Business.Utils;

namespace Casablanca.Business
{
    public class Faturamento : BusinessComponentCasablanca<Faturamento>
    {
        public ConsultaFaturasResponse ConsultarFaturas(ConsultaFaturasRequest request)
        {
            Logger.LogDetail("Método ConsultarFaturas() - In");
            try
            {
                ConsultaFaturasResponse response = new ConsultaFaturasResponse();

                #region [ Validação select faturas ]
                Logger.LogDetail("Consultando comando select da query K_WS_FATURAMENTOFATURAS.");
                Query query = new Query("SELECT COMANDO FROM Z_TABELAS WHERE NOME = :PNOME");
                query.Parameters.Add(new Parameter("PNOME", DataType.String, "K_WS_FATURAMENTOFATURAS"));
                Entities<EntityBase> selectFaturas = query.Execute();

                if (selectFaturas.Count == 0)
                {
                    Logger.LogDetail("Query K_WS_FATURAMENTOFATURAS não encontrada na base de dados.");
                    throw new Exception("Comando para consulta de faturas não encontrado. (Query K_WS_FATURAMENTOFATURAS)");
                }
                #endregion [ Validação select faturas ]

                #region [ Validação select reservas ]
                Logger.LogDetail("Consultando comando select da query K_WS_FATURAMENTOFATURARESERVAS.");
                query.Parameters.Clear();
                query.Parameters.Add(new Parameter("PNOME", DataType.String, "K_WS_FATURAMENTOFATURARESERVAS"));
                Entities<EntityBase> selectFaturaReservas = query.Execute();

                if (selectFaturaReservas.Count == 0)
                {
                    Logger.LogDetail("Query K_WS_FATURAMENTOFATURARESERVAS não encontrada na base de dados.");
                    throw new Exception("Comando para consulta de reservas da fatura não encontrado. (Query K_WS_FATURAMENTOFATURARESERVAS)");
                }
                #endregion [ Validação select reservas ]
                
                #region [ Validação select fees ]
                Logger.LogDetail("Consultando comando select da query K_WS_FATURAMENTOFATURAFEES.");
                query.Parameters.Clear();
                query.Parameters.Add(new Parameter("PNOME", DataType.String, "K_WS_FATURAMENTOFATURAFEES"));
                Entities<EntityBase> selectFaturaFEEs = query.Execute();

                if (selectFaturaFEEs.Count == 0)
                {
                    Logger.LogDetail("Query K_WS_FATURAMENTOFATURAFEES não encontrada na base de dados.");
                    throw new Exception("Comando para consulta fees da fatura não encontrado. (Query K_WS_FATURAMENTOFATURAFEES)");
                }
                #endregion [ Validação select fees ]

                #region [ Validação select reembolsos ]
                Logger.LogDetail("Consultando comando select da query K_WS_FATURAMENTOFATURAREEMBOLS.");
                query.Parameters.Clear();
                query.Parameters.Add(new Parameter("PNOME", DataType.String, "K_WS_FATURAMENTOFATURAREEMBOLS"));
                Entities<EntityBase> selectFaturaReembolsos = query.Execute();

                if (selectFaturaReembolsos.Count == 0)
                {
                    Logger.LogDetail("Query K_WS_FATURAMENTOFATURAREEMBOLS não encontrada na base de dados.");
                    throw new Exception("Comando para consulta reembolsos da fatura não encontrado. (Query K_WS_FATURAMENTOFATURAREEMBOLS)");
                }
                #endregion [ Validação select reembolsos ]

                #region [ Validação dos filtros ]

                Logger.LogDetail(string.Format("Request.: {0}", CBUtils.ToXml(request)));

                response.Excecoes = new List<string>();
                if ((request.DataInicial == null) || (request.DataInicial == DateTime.MinValue))
                    response.Excecoes.Add("Data inicial é obrigatório!");
                
                if ((request.DataFinal == null) || (request.DataFinal == DateTime.MinValue))
                    response.Excecoes.Add("Data final é obrigatório!");
                
                if ((request.DataInicial != null) && (request.DataFinal != null))
                {
                    if (request.DataInicial > request.DataFinal)
                        response.Excecoes.Add("Data inicial não pode ser maior que data final!");

                    TimeSpan ts = request.DataFinal.Subtract(request.DataInicial);
                    if (ts.Days > 180)
                        response.Excecoes.Add("Período máximo de pesquisa permitido é de 180 dias!");
                }

                Logger.LogDetail(string.Format("Exceções request.: {0}", response.Excecoes.toString2()));

                #endregion [ Validação dos filtros ]

                if (response.Excecoes.Count == 0)
                {
                    query = new Query(selectFaturas.FirstOrDefault()["COMANDO"].Value.ToString());
                    
                    #region [ Filtro data ]

                    string campoData = string.Empty;
                    switch (request.TipoData)
                    {
                        case TipoData.DataConfirmacao:
                            campoData = "FAT.DATACONFIRMACAO";
                            break;
                        case TipoData.DataVencimento:
                            campoData = "FAT.DATAVENCIMENTO";
                            break;
                        default:
                            campoData = "FAT.DATA";
                            break;
                    }
                    query.CommandText.AppendFormat(" AND ({0} BETWEEN :PDATAINICIAL AND :PDATAFINAL) ", campoData);
                    query.Parameters.Add(new Parameter("PDATAINICIAL", DataType.DateTime, request.DataInicial));
                    query.Parameters.Add(new Parameter("PDATAFINAL", DataType.DateTime, request.DataFinal));

                    #endregion [ Filtro data ]

                    #region [ Filtro status ]

                    if (request.Status != StatusFatura.Indefinido)
                    {
                        query.CommandText.Append(" AND (FAT.STATUS = :PSTATUS) ");
                        query.Parameters.Add(new Parameter("PSTATUS", DataType.String, (int)request.Status));
                    }

                    #endregion [ Filtro status]
                    Logger.LogDetail(string.Format("Comando utilizado para consulta de faturas.: {0}", query.CommandText));
                    Entities<EntityBase> faturas = query.Execute();

                    if (faturas.Count == 0)
                        response.Excecoes.Add("Nenhuma fatura encontrada com o filtro informado!");
                    else
                    {
                        #region [ Prepara consulta de reservas da fatura ]
                        Query qReservas = new Query(selectFaturaReservas.FirstOrDefault()["COMANDO"].Value.ToString());
                        Entities<EntityBase> faturaReservas;
                        #endregion [ Prepara consulta de reservas da fatura ]

                        #region [ Prepara consulta de fees da fatura ]
                        Query qFEEs = new Query(selectFaturaFEEs.FirstOrDefault()["COMANDO"].Value.ToString());
                        Entities<EntityBase> faturaFEEs;
                        #endregion [ Prepara consulta de fees da fatura ]

                        #region [ Prepara consulta de reembolsos da fatura ]
                        Query qReembolsos = new Query(selectFaturaReembolsos.FirstOrDefault()["COMANDO"].Value.ToString());
                        Entities<EntityBase> faturaReembolsos;
                        #endregion [ Prepara consulta de reembolsos da fatura ]

                        Fatura fatura;
                        response.Faturas = new List<Fatura>();
                        foreach (EntityBase itemFatura in faturas)
                        {
                            fatura = new Fatura();
                            fatura.CNPJCliente = itemFatura["CNPJCLIENTE"].Value.toString2();
                            fatura.NomeCliente = itemFatura["NOMECLIENTE"].Value.toString2();
                            fatura.CodigoCasablanca = itemFatura["HANDLE"].Value.toInt();
                            fatura.Numero = itemFatura["NUMERO"].Value.toString2();
                            fatura.Status = GetStatusFatura(itemFatura["STATUS"].Value.toInt());
                            fatura.DataCriacao = itemFatura["DATACRIACAO"].Value.toDataTime();
                            fatura.DataConfirmacao = itemFatura["DATAEMISSAO"].Value.toDataTime();
                            fatura.DataVencimento = itemFatura["DATAVENCIMENTO"].Value.toDataTime();

                            //Links Fatura e Boleto
                            if (!string.IsNullOrEmpty(itemFatura["ARQUIVOFATURA"].toString2()))
                                fatura.LinkPDFFatura = CBUtils.MontarLinkDownload("BB_FATURAS", "ARQUIVOFATURA", itemFatura["HANDLE"].Value.toLong());
                            if (!string.IsNullOrEmpty(itemFatura["ARQUIVOBOLETO"].toString2()))
                                fatura.LinkPDFBoleto = CBUtils.MontarLinkDownload("BB_FATURAS", "ARQUIVOBOLETO", itemFatura["HANDLE"].Value.toLong());

                            #region [ Preenche reservas ]
                            qReservas.Parameters.Clear();
                            qReservas.Parameters.Add(new Parameter("PFATURA", DataType.Integer, itemFatura["HANDLE"].Value.toInt()));
                            faturaReservas = qReservas.Execute();

                            fatura.Reservas = new List<ItemReserva>();
                            fatura.Reservas.AddRange(PreencherReservasFatura(faturaReservas));
                            #endregion [ Preenche reservas ]

                            #region [ Preenche fees ]
                            qFEEs.Parameters.Clear();
                            qFEEs.Parameters.Add(new Parameter("PFATURA", DataType.Integer, itemFatura["HANDLE"].Value.toInt()));
                            faturaFEEs = qFEEs.Execute();

                            fatura.FEEs = new List<ItemFEE>();
                            fatura.FEEs.AddRange(PreencherFEEsFatura(faturaFEEs));
                            #endregion [ Preenche fees ]

                            #region [ Preenche reembolsos ]
                            qReembolsos.Parameters.Clear();
                            qReembolsos.Parameters.Add(new Parameter("PFATURA", DataType.Integer, itemFatura["HANDLE"].Value.toInt()));
                            faturaReembolsos = qReembolsos.Execute();

                            fatura.Reembolsos = new List<ItemReembolso>();
                            fatura.Reembolsos.AddRange(PreencherReembolsosFatura(faturaReembolsos));
                            #endregion [ Preenche reservas ]

                            response.Faturas.Add(fatura);
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogDetail(string.Format("Erro ao consultar faturas. Erro.: {0}", ex.Message));
                throw new Exception(string.Format("Erro ao consultar faturas. Erro.: {0}", ex.Message));
            }            
            finally
            {
                Logger.LogDetail("Método ConsultarFaturas() - Out");
            }
        }

        private List<ItemReserva> PreencherReservasFatura(Entities<EntityBase> prReservas)
        {
            List<ItemReserva> result = new List<ItemReserva>();

            ItemReserva reserva;
            foreach (EntityBase item in prReservas)
            {
                reserva = new ItemReserva();
                reserva.DataEmissao = item["DATAEMISSAO"].Value.toDataTime();
				reserva.NumeroReserva = item["NUMERORESERVA"].Value.toString2();
				reserva.CodigoReserva = item["CODIGORESERVA"].Value.toString2();
				reserva.InfOS = item["INFOS"].Value.toString2();
				reserva.InfMotivo = item["INFMOTIVO"].Value.toString2();
				reserva.Observacoes = item["OBSERVACOES"].Value.toString2();
				reserva.Fornecedor = item["FORNECEDOR"].Value.toString2();
                reserva.Produto = GetProduto(item["PRODUTO"].Value.toInt());
                reserva.Solicitante = item["SOLICITANTE"].Value.toString2();
                reserva.Passageiro = item["PASSAGEIRO"].Value.toString2();
                reserva.Trecho = item["TRECHO"].Value.toString2();
                reserva.Cidade = item["CIDADE"].Value.toString2();
                reserva.CentroCusto = item["CENTROCUSTO"].Value.toString2();
				reserva.Projeto = item["PROJETO"].Value.toString2();

                if (reserva.Produto == Produto.Aereo)
                {
                    if (item["DATAVIAGEMAEREO"].Value.toDataTime() > DateTime.MinValue)
                        reserva.DataInicial = item["DATAVIAGEMAEREO"].Value.toDataTime();
                    if (item["DATARETORNOAEREO"].Value.toDataTime() > DateTime.MinValue)
                        reserva.DataFinal = item["DATARETORNOAEREO"].Value.toDataTime();
                }
                else
                {
                    if (item["DATAIN"].Value.toDataTime() > DateTime.MinValue)
                        reserva.DataInicial = item["DATAIN"].Value.toDataTime();
                    if (item["DATAOUT"].Value.toDataTime() > DateTime.MinValue)
                        reserva.DataFinal = item["DATAOUT"].Value.toDataTime();
                }
                
                if ((reserva.DataInicial != null && reserva.DataInicial > DateTime.MinValue) &&
                    (reserva.DataFinal != null && reserva.DataFinal > DateTime.MinValue))
                {
                    TimeSpan ts = reserva.DataFinal.Subtract(reserva.DataInicial);
					int diariaCarroExtra = 0;	// Se o carro passa da hora de devolução
/*					if (reserva.Produto == Produto.Carro)
					{
						TimeSpan padrao = new TimeSpan(12, 0, 0);	// As 12:00 do dia, hora de entregue do carro
						TimeSpan timeOfDay = item["BB_CARRO_ENTREGA"].Value.toDataTime().TimeOfDay;	// Hora de entregue real do carro
						if(timeOfDay > padrao) diariaCarroExtra = 1;
					}	*/
					reserva.QuantidadeDiarias = ts.Days + diariaCarroExtra;
                }

                reserva.Tarifa = item["TARIFA"].Value.toDecimal();
                reserva.TaxaEmbarque = item["TAXAEMBARQUE"].Value.toDecimal();
                reserva.TaxaServico = item["TAXASERVICO"].Value.toDecimal();
                reserva.TaxaServicoFEE = item["TAXASERVICOFEE"].Value.toDecimal();
                reserva.TaxaDU = item["TAXADU"].Value.toDecimal();
                reserva.Extras = item["EXTRAS"].Value.toDecimal();
                reserva.OutrasTaxas = item["OUTRASTAXAS"].Value.toDecimal();
                reserva.Desconto = item["DESCONTO"].Value.toDecimal();

                result.Add(reserva);
            }

            return result;
        }

        private List<ItemFEE> PreencherFEEsFatura(Entities<EntityBase> prFEEs)
        {
            List<ItemFEE> result = new List<ItemFEE>();

            ItemFEE fee;
            foreach (EntityBase item in prFEEs)
            {
                fee = new ItemFEE();
                fee.DataEmissao = item["DATAEMISSAO"].Value.toDataTime();
                fee.NumeroReserva = item["NUMERORESERVA"].Value.toString2();
                fee.Fornecedor = item["FORNECEDOR"].Value.toString2();
                fee.Produto = GetProduto(item["PRODUTO"].Value.toInt());
                fee.Solicitante = item["SOLICITANTE"].Value.toString2();
                fee.Passageiro = item["PASSAGEIRO"].Value.toString2();
                fee.CentroCusto = item["CENTROCUSTO"].Value.toString2();
                fee.ValorFEE = item["FEE"].Value.toDecimal();
                fee.Trechos = item["TRECHOS"].Value.toString2();
                fee.DataIni = item["DATAINI"].Value.toDataTime();
                fee.DataFim = item["DATAFIM"].Value.toDataTime();
                

                result.Add(fee);
            }

            return result;
        }

        private List<ItemReembolso> PreencherReembolsosFatura(Entities<EntityBase> prReembolsos)
        {
            List<ItemReembolso> result = new List<ItemReembolso>();

            ItemReembolso reembolso;
            foreach (EntityBase item in prReembolsos)
            {
                reembolso = new ItemReembolso();
                reembolso.DataEmissao = item["DATAEMISSAO"].Value.toDataTime();
                reembolso.NumeroReserva = item["NUMERORESERVA"].Value.toString2();
                reembolso.Fornecedor = item["FORNECEDOR"].Value.toString2();
                reembolso.Produto = GetProduto(item["PRODUTO"].Value.toInt());
                reembolso.Solicitante = item["SOLICITANTE"].Value.toString2();
                reembolso.Passageiro = item["PASSAGEIRO"].Value.toString2();
                reembolso.Trecho = item["TRECHO"].Value.toString2();
                reembolso.CentroCusto = item["CENTROCUSTO"].Value.toString2();
                reembolso.Tarifa = item["TARIFA"].Value.toDecimal();
                reembolso.Taxa = item["TAXA"].Value.toDecimal();
                reembolso.Multa = item["MULTA"].Value.toDecimal();
                reembolso.ValorReembolso = item["REEMBOLSO"].Value.toDecimal();

                result.Add(reembolso);
            }

            return result;
        }

        private StatusFatura GetStatusFatura(int p)
        {
            switch (p)
            {
                case 1:
                    return StatusFatura.Nova;
                case 2:
                    return StatusFatura.Cadastrada;
                case 3:
                    return StatusFatura.Gerada;
                case 4:
                    return StatusFatura.Emitida;
                case 5:
                    return StatusFatura.Cancelada;
                case 6:
                    return StatusFatura.Liquidada;
            }
            return StatusFatura.Nova;
        }

        private Produto GetProduto(int p)
        {
            switch (p)
            {
                case 1:
                case 5:
                case 6:
                case 7:
                    return Produto.Aereo;
                case 2:
                    return Produto.Hotel;
                case 3:
                    return Produto.Carro;
                default:
                    return Produto.Servicos;
            }
        }
    }
}