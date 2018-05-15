using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Casablanca.DataContracts.Reservas;
using Benner.Tecnologia.Common.Instrumentation;
using Casablanca.Business.Utils;
using Benner.Tecnologia.Business;
using Benner.Tecnologia.Common;
using Casablanca.DataContracts.Faturamento;

namespace Casablanca.Business
{
    public class ReservasBase : BusinessComponentCasablanca<ReservasBase>
    {
        public ConsultaReservasResponse ConsultarReservas(ConsultaReservasRequest request)
        {
            Logger.LogDetail("Método ConsultarReservas() - In");
            try
            {
                ConsultaReservasResponse response = new ConsultaReservasResponse();

                #region [ Validação dos filtros ]

                Logger.LogDetail(string.Format("Request: {0}", CBUtils.ToXml(request)));

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

                Logger.LogDetail(string.Format("Exceções request: {0}", response.Excecoes.toString2()));

                #endregion [ Validação dos filtros ]

                if (response.Excecoes.Count == 0)
                {
                    Query query = new Query(@"
                        SELECT ACC.DATAEMISSAO, PNR.BB_RLOC NUMERORESERVA, GPP.NOME GRUPOEMPRESARIAL,
                        CLI.NOME CLIENTE, ACC.PASSAGEIRONAOCAD PASSAGEIRO, ACC.PRODUTO,
                        CASE WHEN ACC.PRODUTO IN (1,5,6,7) THEN
                          (SELECT MIN(B.BB_AEREO_DATA_VOO)
                           FROM BB_TRECHOACCOUNTING A
                           INNER JOIN BB_PNRTRECHOS B ON (B.HANDLE = A.TRECHO)
                           WHERE A.ACCOUNTING = ACC.HANDLE)
                        WHEN ACC.PRODUTO = 2 THEN
                          (ACC.BB_HOTEL_ENTRADA)
                        WHEN ACC.PRODUTO = 3 THEN
                          (ACC.BB_CARRO_RETIRADA)
                        WHEN ACC.PRODUTO = 4 THEN
                          (ACC.BB_MISC_ENTRADA)
                        END DATAINICIAL,
                        CASE WHEN ACC.PRODUTO IN (1,5,6,7) THEN
                          (SELECT MAX(B.BB_AEREO_DATA_VOO)
                           FROM BB_TRECHOACCOUNTING A
                           INNER JOIN BB_PNRTRECHOS B ON (B.HANDLE = A.TRECHO)
                           WHERE A.ACCOUNTING = ACC.HANDLE)
                        WHEN ACC.PRODUTO = 2 THEN
                          (ACC.BB_HOTEL_SAIDA)
                        WHEN ACC.PRODUTO = 3 THEN
                          (ACC.BB_CARRO_ENTREGA)
                        WHEN ACC.PRODUTO = 4 THEN
                          (ACC.BB_MISC_SAIDA)
                        END DATAFINAL,
                        MUNFOR.NOME MUNICIPIO, ESTFOR.NOME ESTADO, REGEST.NOME REGIAO, PAISFOR.NOME PAIS, REGPAIS.NOME CONTINENTE
                        FROM BB_PNRACCOUNTINGS ACC
                        INNER JOIN BB_PNRS PNR ON (PNR.HANDLE = ACC.RLOC)
                        INNER JOIN GN_PESSOAS CLI ON (CLI.HANDLE = ACC.BB_CLIENTE)
                        LEFT OUTER JOIN GN_GRUPOSEMPRESARIAIS GP ON (GP.HANDLE = CLI.GRUPOEMPRESARIAL)
                        LEFT OUTER JOIN GN_PESSOAS GPP ON (GPP.HANDLE = GP.PESSOA)
                        INNER JOIN GN_PESSOAS FORN ON (FORN.HANDLE = ACC.FORNECEDOR)
                        LEFT OUTER JOIN MUNICIPIOS MUNFOR ON (MUNFOR.HANDLE = FORN.MUNICIPIO)
                        LEFT OUTER JOIN ESTADOS ESTFOR ON (ESTFOR.HANDLE = FORN.ESTADO)
                        LEFT OUTER JOIN PAISES PAISFOR ON (PAISFOR.HANDLE = FORN.PAIS)
                        LEFT OUTER JOIN K_REGIOES REGEST ON (REGEST.HANDLE = ESTFOR.K_REGIAO)
                        LEFT OUTER JOIN K_REGIOES REGPAIS ON (REGPAIS.HANDLE = PAISFOR.K_CONTINENTE) 
                        WHERE ACC.DATAEMISSAO BETWEEN :PDATAINICIAL AND :PDATAFINAL 
                        AND ((ACC.BB_CLIENTE IN (SELECT PES.HANDLE
                                                 FROM GN_PESSOAS PES
                                                 WHERE PES.GRUPOEMPRESARIAL IN (SELECT PESSOA
						                                                        FROM BB_USUARIOPESSOAS
						                                                        WHERE USUARIO = @USUARIO
                                                                                AND GRUPOEMPRESARIAL = 'S'))
                             OR ACC.BB_CLIENTE IN (SELECT PESSOA
		                                           FROM BB_USUARIOPESSOAS
                                                   WHERE USUARIO = @USUARIO))) ");
                    
                    query.Parameters.Add(new Parameter("PDATAINICIAL", DataType.DateTime, request.DataInicial));
                    query.Parameters.Add(new Parameter("PDATAFINAL", DataType.DateTime, request.DataFinal));


                    Logger.LogDetail(string.Format("Comando utilizado para consulta de reservas: {0}", query.CommandText));
                    Entities<EntityBase> reservas = query.Execute();

                    if (reservas.Count == 0)
                        response.Excecoes.Add("Nenhuma reserva encontrada com o filtro informado!");
                    else
                    {
                        Reserva reserva;
                        response.Reservas = new List<Reserva>();
                        foreach (EntityBase itemReserva in reservas)
                        {
                            reserva = new Reserva();
                            reserva.DataEmissao = itemReserva["DATAEMISSAO"].Value.toDataTime();
                            reserva.NumeroReserva = itemReserva["NUMERORESERVA"].Value.toString2();
                            reserva.GrupoEmpresarial = itemReserva["GRUPOEMPRESARIAL"].Value.toString2();
                            reserva.Cliente = itemReserva["CLIENTE"].Value.toString2();
                            reserva.Passageiro = itemReserva["PASSAGEIRO"].Value.toString2();
                            reserva.Produto = GetProduto(itemReserva["PRODUTO"].Value.toInt());
                            reserva.DataInicial = itemReserva["DATAINICIAL"].Value.toDataTime();
                            reserva.DataFinal = itemReserva["DATAFINAL"].Value.toDataTime();
                            if ((reserva.DataInicial != null && reserva.DataInicial > DateTime.MinValue) &&
                                (reserva.DataFinal != null && reserva.DataFinal > DateTime.MinValue))
                            {
                                TimeSpan ts = reserva.DataFinal.Subtract(reserva.DataInicial);
                                reserva.QuantidadeDiarias = ts.Days;
                            }
                            reserva.Municipio = itemReserva["MUNICIPIO"].Value.toString2();
                            reserva.Estado = itemReserva["ESTADO"].Value.toString2();
                            reserva.Regiao = itemReserva["REGIAO"].Value.toString2();
                            reserva.Pais = itemReserva["PAIS"].Value.toString2();
                            reserva.Continente = itemReserva["CONTINENTE"].Value.toString2();

                            response.Reservas.Add(reserva);
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogDetail(string.Format("Erro ao consultar reservas. Erro: {0}", ex.Message));
                throw new Exception(string.Format("Erro ao consultar reservas. Erro: {0}", ex.Message));
            }            
            finally
            {
                Logger.LogDetail("Método ConsultarReservas() - Out");
            }
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
