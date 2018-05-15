using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Casablanca.DataContracts.Adiantamento;
using Benner.Tecnologia.Common.Instrumentation;
using Casablanca.Business.Utils;
using Benner.Tecnologia.Business;
using Benner.Tecnologia.Common;
using Casablanca.DataContracts.Faturamento;

namespace Casablanca.Business
{
	public class Adiantamento : BusinessComponentCasablanca<Adiantamento>
	{
		public ConsultaAdiantamentoResponse ConsultarAdiantamento(ConsultaAdiantamentoRequest request)
		{
			string debug = "001";
			Logger.LogDetail("Método ConsultarAdiantamento() - In");
			try
			{
				ConsultaAdiantamentoResponse response = new ConsultaAdiantamentoResponse();
				debug = "002";

				#region [ Validação dos filtros ]

				Logger.LogDetail(string.Format("Request: {0}", CBUtils.ToXml(request)));
				debug = "003";

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
				debug = "004";

				if (response.Excecoes.Count == 0)
				{
					//	LER HOTEIS E AEREOS
					//	MONTAR A BASE DE VIAGENS
					//	LER VIAGEM A VIAGEM
					//	LER INFORMAÇÕES QUE ENTREM NAS MESMAS DATAS PARA OS MESMOS VIAJEIROS
					//	MONTAR O WEBSERVICE



					Query query = new Query(@"
						DECLARE @DATAINICIO AS DATE
						SET @DATAINICIO = :PDATAINICIAL

						DECLARE @DATAFINAL AS DATE
						SET @DATAFINAL = :PDATAFINAL

						DECLARE @SITUACOES TABLE(HANDLE int, SITUACAO varchar(15))
						INSERT INTO @SITUACOES (HANDLE, SITUACAO) values (1, 'Ativo'), (2, 'Cancelado')

						DECLARE @PRODUTOS TABLE(HANDLE int, PRODUTO varchar(15))
						INSERT INTO @PRODUTOS (HANDLE, PRODUTO) values 
							(1, 'Aéreo'),
							(2, 'Hotel'),
							(3, 'Carro'),
							(4, 'Miscelaneo'),
							(5, 'Aéreo'),
							(6, 'Aéreo'),
							(7, 'Aéreo'),
							(8, 'Rodoviário')

						DECLARE @TIPOSEMISSAO TABLE(HANDLE varchar(1), TIPOEMISSAO varchar(15))
						INSERT INTO @TIPOSEMISSAO (HANDLE, TIPOEMISSAO) values ('A', 'Ativo'), ('E', 'Externa'), ('M', 'Manual')

						DECLARE @TIPOSVOO TABLE(HANDLE varchar(1), TIPOVOO varchar(15))
						INSERT INTO @TIPOSVOO (HANDLE, TIPOVOO) values ('D', 'Doméstico'), ('F', 'Internacional'), ('T', 'Regional')

						DECLARE @FORMASPAGAMENTO TABLE(HANDLE int, FORMAPAGAMENTO varchar(20))
						INSERT INTO @FORMASPAGAMENTO (HANDLE, FORMAPAGAMENTO) values 
							(1, 'Cartão'),
							(2, 'Invoice'),
							(3, 'Outras'),
							(4, 'GR'),
							(5, 'Múltiplos pagamentos')

						DECLARE @FORMASRECEBIMENTO TABLE(HANDLE int, FORMARECEBIMENTO varchar(30))
						INSERT INTO @FORMASRECEBIMENTO (HANDLE, FORMARECEBIMENTO) values 
							(1, 'Cash'),
							(3, 'Faturado'),
							(5, 'Cheque'),
							(6, 'Pagamento direto'),
							(7, 'Cartão'),
							(9, 'Cartão convênio'),
							(10, 'Sem adicional'),
							(11, 'Cartão AMEX'),
							(12, 'Governo'),
							(13, 'Cheque pré-datado'),
							(14, 'As financiadas por terceiros'),
							(15, 'Múltiplos recebimentos'),
							(16, 'Despesas internas'),
							(17, 'TKT'),
							(18, 'MCO')

						USE TURISMOPRD

						SELECT
						--	CENTRO DE CUSTO
							CLI_CC.CENTROCUSTO AS CC_CLIENTE,
							CONVERT(VARCHAR(50), CLI.NOME) AS CLIENTE,
						--	VIAJANTES
						--	VIAJANTE
							CASE A.TIPOPASSAGEIRO WHEN 1 THEN CONVERT(VARCHAR(50), A.PASSAGEIRONAOCAD) WHEN 2 THEN CONVERT(VARCHAR(50), PAX.NOME) END AS PASSAGEIRO,
							ISNULL(PAX.CGCCPF, 'Não cadastrado') AS CPF,
						--	VIAGENS
						--	VIAGEM
							ISNULL(A.TRECHOS, ISNULL(ISNULL(TRE.BB_AEREO_DESTINO, DES_MUN.SIGLA), '')) AS TRECHOS,
						--	ORIGEM
							CONVERT(DATETIME, ISNULL(TRE.BB_AEREO_DATA_VOO, ISNULL(A.BB_MISC_ENTRADA, ISNULL(A.BB_CARRO_RETIRADA, A.BB_HOTEL_ENTRADA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '00:00:00')) AS DATA_ORIGEM,
							ISNULL(TRE.BB_AEREO_ORIGEM, '') AS AEREO_ORIGEM,
							ISNULL(ORI_MUN.NOME, '') AS MUNICIPIO_ORIGEM,
							ISNULL(CASE ORI_PAIS.NOME WHEN 'Brasil' THEN ORI_EST.NOME ELSE NULL END, '') AS ESTADO_ORIGEM,
							ISNULL(CASE ORI_PAIS.NOME WHEN 'Brasil' THEN ORI_REG.NOME ELSE NULL END, '') AS REGIAO_ORIGEM,
							ISNULL(ORI_PAIS.NOME, '') AS PAIS_ORIGEM,
							ISNULL(CASE ORI_PAIS.NOME WHEN 'Brasil' THEN 'América Do Sul' ELSE ORI_REG.NOME END, '') AS CONTINENTE_ORIGEM,
						--	DESTINO
							CONVERT(DATETIME, ISNULL((CASE WHEN CONVERT(DATETIME, TRE.BB_AEREO_DATA_VOO + TRE.BB_AEREO_HRSAIDA) > CONVERT(DATETIME, TRE.BB_AEREO_DATA_VOO + TRE.BB_AEREO_HRCHEGADA) THEN DATEADD(DAY, 1, TRE.BB_AEREO_DATA_VOO) ELSE TRE.BB_AEREO_DATA_VOO END), ISNULL(A.BB_MISC_SAIDA, ISNULL(A.BB_CARRO_ENTREGA, A.BB_HOTEL_SAIDA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '')) AS DATA_DESTINO,
							ISNULL(ISNULL(TRE.BB_AEREO_DESTINO, DES_MUN.SIGLA), '') AS AEREO_DESTINO,
							ISNULL(DES_MUN.NOME, '') AS MUNICIPIO_DESTINO,
							ISNULL(CASE DES_PAIS.NOME WHEN 'Brasil' THEN DES_EST.NOME ELSE NULL END, '') AS ESTADO_DESTINO,
							ISNULL(CASE DES_PAIS.NOME WHEN 'Brasil' THEN DES_REG.NOME ELSE NULL END, '') AS REGIAO_DESTINO,
							ISNULL(DES_PAIS.NOME, '') AS PAIS_DESTINO,
							ISNULL(CASE DES_PAIS.NOME WHEN 'Brasil' THEN 'América Do Sul' ELSE DES_REG.NOME END, '') AS CONTINENTE_DESTINO,
						--	RESERVAS
						--	RESERVA
							CASE A.PRODUTO WHEN 4 THEN TIPMIS.NOME ELSE PRO.PRODUTO END AS PRODUTO,
							ISNULL(TVOO.TIPOVOO, '')  AS TIPO,
							PNR.BB_RLOC AS RLOC,
							PNR.LOCALIZADORACIA AS LOC,
							ISNULL(A.BILHETE, '') AS BILHETE,
							ISNULL(A.CONFIRMACAO, '') AS CONFIRMACAO,
							CONVERT(DATE, A.DATAEMISSAO) AS DATAEMISSAO,
							AGE.NOME AS EMISSOR_NOME,
							A.INFAPROVADOR AS APROVADOR,
							ISNULL(CONS.NOME, '') AS CONSOLIDADORA,
							CONVERT(VARCHAR(50), FORN.NOME) AS FORNECEDOR
						FROM BB_PNRACCOUNTINGS A
							INNER JOIN BB_PNRS PNR ON PNR.HANDLE = A.RLOC
							INNER JOIN GN_PESSOAS CLI ON CLI.HANDLE = A.BB_CLIENTE
							LEFT JOIN BB_CLIENTECC AS CLI_CC ON A.CENTRODECUSTO = CLI_CC.HANDLE
							LEFT OUTER JOIN GN_GRUPOSEMPRESARIAIS GP ON GP.HANDLE = CLI.GRUPOEMPRESARIAL
							LEFT OUTER JOIN GN_PESSOAS GPP ON GPP.HANDLE = GP.PESSOA
							INNER JOIN GN_PESSOAS FORN ON FORN.HANDLE = A.FORNECEDOR
							LEFT JOIN GN_PESSOAS AS CONS ON A.CONSOLIDADOR = CONS.HANDLE
							LEFT JOIN @SITUACOES AS SIT ON SIT.HANDLE = A.SITUACAO
							LEFT JOIN BB_AGENTES AS AGE ON A.AGENTE_EMISSAO = AGE.HANDLE
							LEFT JOIN @PRODUTOS AS PRO ON PRO.HANDLE = A.PRODUTO
							LEFT JOIN BB_TIPOMISCELANIO AS TIPMIS ON A.TIPOMISCELANIO = TIPMIS.HANDLE
							LEFT JOIN @TIPOSEMISSAO AS TEMI ON TEMI.HANDLE = A.TIPODEEMISSAO
							LEFT JOIN @TIPOSVOO AS TVOO ON TVOO.HANDLE = A.TIPOVOO
							LEFT JOIN GN_PESSOAS AS PAX ON A.VIAJANTE = PAX.HANDLE
							LEFT JOIN BB_TRECHOACCOUNTING TRE_A ON A.HANDLE = TRE_A.ACCOUNTING
							LEFT JOIN BB_PNRTRECHOS AS TRE ON TRE.HANDLE = TRE_A.TRECHO
							LEFT JOIN BB_AEROPORTOS AS DES_AERO ON DES_AERO.AEROPORTO = TRE.BB_AEREO_DESTINO
							LEFT JOIN MUNICIPIOS AS DES_MUN ON (PRO.PRODUTO = 'Aéreo' AND DES_MUN.HANDLE = DES_AERO.CIDADE) OR (PRO.PRODUTO <> 'Aéreo' AND DES_MUN.HANDLE = FORN.MUNICIPIO)
							LEFT JOIN ESTADOS DES_EST ON DES_EST.HANDLE = DES_MUN.ESTADO
							LEFT JOIN PAISES DES_PAIS ON DES_PAIS.HANDLE = DES_MUN.PAIS
							LEFT JOIN K_REGIOES DES_REG ON DES_REG.HANDLE = DES_PAIS.K_CONTINENTE OR DES_REG.HANDLE = DES_EST.K_REGIAO
							LEFT JOIN BB_AEROPORTOS AS ORI_AERO ON ORI_AERO.AEROPORTO = TRE.BB_AEREO_ORIGEM
							LEFT JOIN MUNICIPIOS AS ORI_MUN ON ORI_MUN.HANDLE = ORI_AERO.CIDADE
							LEFT JOIN ESTADOS ORI_EST ON ORI_EST.HANDLE = ORI_MUN.ESTADO
							LEFT JOIN PAISES ORI_PAIS ON ORI_PAIS.HANDLE = ORI_MUN.PAIS
							LEFT JOIN K_REGIOES ORI_REG ON ORI_REG.HANDLE = ORI_PAIS.K_CONTINENTE OR ORI_REG.HANDLE = ORI_EST.K_REGIAO
							LEFT JOIN @FORMASPAGAMENTO AS FPAG ON FPAG.HANDLE = A.FORMADEPAGAMENTO
							LEFT JOIN @FORMASRECEBIMENTO AS FREC ON FREC.HANDLE = A.FORMARECEBIMENTO
							LEFT JOIN BB_MAQUINAS AS PCCAGE ON A.PCC_EMISSAO = PCCAGE.HANDLE
							LEFT JOIN BB_CCPROJETOACC AS CCPROJ ON A.HANDLE = CCPROJ.ACCOUNTING
							LEFT JOIN CT_CC AS CC ON CCPROJ.CCSUGERIDO = CC.HANDLE
						WHERE CONVERT(DATETIME, ISNULL(TRE.BB_AEREO_DATA_VOO, ISNULL(A.BB_MISC_ENTRADA, ISNULL(A.BB_CARRO_RETIRADA, A.BB_HOTEL_ENTRADA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '00:00:00')) <= @DATAFINAL
							AND CONVERT(DATETIME, ISNULL((CASE WHEN CONVERT(DATETIME, TRE.BB_AEREO_DATA_VOO + TRE.BB_AEREO_HRSAIDA) > CONVERT(DATETIME, TRE.BB_AEREO_DATA_VOO + TRE.BB_AEREO_HRCHEGADA) THEN DATEADD(DAY, 1, TRE.BB_AEREO_DATA_VOO) ELSE TRE.BB_AEREO_DATA_VOO END), ISNULL(A.BB_MISC_SAIDA, ISNULL(A.BB_CARRO_ENTREGA, A.BB_HOTEL_SAIDA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '')) >= @DATAINICIO
							AND ((A.BB_CLIENTE IN (
								SELECT PES.HANDLE
								FROM GN_PESSOAS PES
								WHERE PES.GRUPOEMPRESARIAL IN (
									SELECT PESSOA
									FROM BB_USUARIOPESSOAS
									WHERE USUARIO = @USUARIO
									AND GRUPOEMPRESARIAL = 'S'))
								OR A.BB_CLIENTE IN (
									SELECT PESSOA
									FROM BB_USUARIOPESSOAS
									WHERE USUARIO = @USUARIO)))
							AND A.SITUACAO = 1
							AND PNR.REEMISSAO = 'N'
							AND PRO.PRODUTO IN ('Aéreo', 'Hotel')
						ORDER BY CC_CLIENTE, PASSAGEIRO, DATA_ORIGEM");

					query.Parameters.Add(new Parameter("PDATAINICIAL", DataType.DateTime, request.DataInicial));
					query.Parameters.Add(new Parameter("PDATAFINAL", DataType.DateTime, request.DataFinal));
					debug = "005";


					Logger.LogDetail(string.Format("Comando utilizado para consulta de viagens: {0}", query.CommandText));
					Entities<EntityBase> dados = query.Execute();
					if (dados.Count == 0)
						response.Excecoes.Add("Nenhuma viagem encontrada com o filtro informado!");
					else
					{
						debug = "006";
						Reserva Reserva;
						Viagem Viagem;
						Viajante Viajante;
						CentroCusto CentroCusto;

						Reserva = new Reserva();
						Viagem = new Viagem();
						Viajante = new Viajante();
						Viajante.Viagens = new List<Viagem>();

						CentroCusto = new CentroCusto();
						CentroCusto.Viajantes = new List<Viajante>();

						response.CentrosCusto = new List<CentroCusto>();

						Viagem.Origem = new Local();
						Viagem.Destino = new Local();
						Viagem.Reservas = new List<Reserva>();

						Reserva.Origem = new Local();
						Reserva.Destino = new Local();

						debug = "006-3";
						foreach (EntityBase item in dados)
						{
							Reserva.Produto = item["PRODUTO"].Value.toString2();
							Reserva.Tipo = item["TIPO"].Value.toString2();
							Reserva.CodigoCasablanca = item["RLOC"].Value.toString2().ToUpper();
							Reserva.Loc = item["LOC"].Value.toString2().ToUpper();
							Reserva.Bilhete = item["BILHETE"].Value.toString2();
							Reserva.Confirmacao = item["CONFIRMACAO"].Value.toString2();
							Reserva.DataEmissao = item["DATAEMISSAO"].Value.toDataTime();
							Reserva.Emissor = item["EMISSOR_NOME"].Value.toString2();
							Reserva.Aprovador = item["APROVADOR"].Value.toString2();
							Reserva.Consolidador = item["CONSOLIDADORA"].Value.toString2();
							Reserva.Fornecedor = item["FORNECEDOR"].Value.toString2();

							Reserva.Origem.DataHora = item["DATA_ORIGEM"].Value.toDataTime();
							Reserva.Origem.Aeroporto = item["AEREO_ORIGEM"].Value.toString2();
							Reserva.Origem.Municipio = item["MUNICIPIO_ORIGEM"].Value.toString2();
							Reserva.Origem.Estado = item["ESTADO_ORIGEM"].Value.toString2();
							Reserva.Origem.Regiao = item["REGIAO_ORIGEM"].Value.toString2();
							Reserva.Origem.Pais = item["PAIS_ORIGEM"].Value.toString2();
							Reserva.Origem.Continente = item["CONTINENTE_ORIGEM"].Value.toString2();

							Reserva.Destino.DataHora = item["DATA_DESTINO"].Value.toDataTime();
							Reserva.Destino.Aeroporto = item["AEREO_DESTINO"].Value.toString2();
							Reserva.Destino.Municipio = item["MUNICIPIO_DESTINO"].Value.toString2();
							Reserva.Destino.Estado = item["ESTADO_DESTINO"].Value.toString2();
							Reserva.Destino.Regiao = item["REGIAO_DESTINO"].Value.toString2();
							Reserva.Destino.Pais = item["PAIS_DESTINO"].Value.toString2();
							Reserva.Destino.Continente = item["CONTINENTE_DESTINO"].Value.toString2();

							debug = "006-3c";
							Viagem.Reservas.Add(Reserva);
							debug = "006-3d";
							Viajante.Viagens.Add(Viagem);
							debug = "006-3e";
							CentroCusto.Viajantes.Add(Viajante);
							debug = "006-3f";
							response.CentrosCusto.Add(CentroCusto);
							debug = "009b";
						}
					}
				}





/*
						// VARIAVES DE COMPARAÇÃO
//						string last_rloc = null;
//						int last_rloc_pos = 0;
						string last_CC = null;
						string last_viajante = null;									// No futuro será o CPF - 
//						string last_origem = null;
//						bool tem_hotel = false;
						DateTime null_data = new System.DateTime(2000, 1, 1, 0, 0, 0);	// Isto vai ser equivalente a datetime nula
						DateTime first_data = null_data;								// Aquí é para gerar uma variavel datetime nula
						DateTime last_data = null_data;									// Aquí é para gerar uma variavel datetime nula

//						CONFERIR SE É HOTEL.
//						SE É, MARCAR O DESTINO CONFERIR AS RESERVAS ANTERIORES
//						ONDE ENCONTRAR O MESMO DESTINO ADICIONAR NAS RESERVAS.
//							ONDE FOR AVIÃO, PEGAR O RLOC
//								ONDE FOR AVIÃO OU COM O MESMO RLOC, ADICIONAR NAS RESERVAS
//						COLOCAR INICIO DA VIAGEM NO PRIMEIRO ORIGEM
//						COLOCAR AS DIARIAS A DIFERENCIA ENTRE O PRIMEIRO ORIGEM E O ÚLTIMO DESTINO
//						Tem que ser tempo tudo o mesmo viajante e o mesmo centro de custo
//------------------------------------------------------------------------------
						debug = "007";

						response.CentrosCusto = new List<CentroCusto>();
						Adiantamento = new ConsultaAdiantamentoResponse();

						CentroCusto = new CentroCusto();
						CentroCusto.Viajantes = new List<Viajante>();

						Viajante = new Viajante();
						Viajante.Viagens = new List<Viagem>();

						Viagem = new Viagem();
						Viagem.Origem = new Endereco();
						Viagem.Destino = new Endereco();
						Viagem.Reservas = new List<Reserva>();

						Reserva = new Reserva();
						Reserva.Origem = new Endereco();
						Reserva.Destino = new Endereco();

						foreach (EntityBase item in dados)
						{
							debug = "008";
							Reserva.CodigoCasablanca = item["RLOC"].Value.toString2().ToUpper();
							Reserva.Loc = item["LOC"].Value.toString2().ToUpper();
							Reserva.Bilhete = item["BILHETE"].Value.toString2();
							Reserva.Confirmacao = item["CONFIRMACAO"].Value.toString2();
							Reserva.Situacao = item["SITUACAO"].Value.toString2();
							Reserva.Reemisao = item["REEMISSAO"].Value.toString2();
							Reserva.Aprovador = item["APROVADOR"].Value.toString2();
							Reserva.Produto = item["PRODUTO"].Value.toString2();
							Reserva.Fornecedor = item["FORNECEDOR"].Value.toString2();
							Reserva.TipoVoo = item["TIPO"].Value.toString2();
							Reserva.Origem.DataHora = item["DATA_ORIGEM"].Value.toDataTime();
							Reserva.Destino.DataHora = item["DATA_DESTINO"].Value.toDataTime();

							Reserva.Origem.Aeroporto = item["AEREO_ORIGEM"].Value.toString2();
							Reserva.Origem.Municipio = item["MUNICIPIO_ORIGEM"].Value.toString2();
							Reserva.Origem.Estado = item["ESTADO_ORIGEM"].Value.toString2();
							Reserva.Origem.Regiao = item["REGIAO_ORIGEM"].Value.toString2();
							Reserva.Origem.Pais = item["PAIS_ORIGEM"].Value.toString2();
							Reserva.Origem.Continente = item["CONTINENTE_ORIGEM"].Value.toString2();
							Reserva.Destino.Aeroporto = item["AEREO_DESTINO"].Value.toString2();
							Reserva.Destino.Municipio = item["MUNICIPIO_DESTINO"].Value.toString2();
							Reserva.Destino.Estado = item["ESTADO_DESTINO"].Value.toString2();
							Reserva.Destino.Regiao = item["REGIAO_DESTINO"].Value.toString2();
							Reserva.Destino.Pais = item["PAIS_DESTINO"].Value.toString2();
							Reserva.Destino.Continente = item["CONTINENTE_DESTINO"].Value.toString2();

//							Decissão para colocar a reserva numa viagem específica ou avulsa
							debug = "009";
							Viagem.Reservas.Add(Reserva);
							debug = "009b";

							if(Reserva.Produto == "Hotel") Viagem.Destino = Reserva.Destino;
							else
							{
								Viagem.Origem = Reserva.Origem;
								Viagem.Destino = Reserva.Destino;
							}

							Reserva = new Reserva();
							Reserva.Origem = new Endereco();
							Reserva.Destino = new Endereco();

							if (last_viajante == null)
							{
								last_viajante = item["PASSAGEIRO"].Value.toString2();
								Viajante.Passageiro = last_viajante;
								Viajante.CPF = item["CPF"].Value.toString2();
							}

							if (last_CC == null)
							{
								last_CC = item["CC_CLIENTE"].Value.toString2();
								CentroCusto.Nome = last_CC;
								CentroCusto.Cliente = item["CLIENTE"].Value.toString2();
							}

							if (last_viajante != item["PASSAGEIRO"].Value.toString2())
							{
								Viajante.Viagens.Add(Viagem);
								CentroCusto.Viajantes.Add(Viajante);
								Viagem = new Viagem();
								Viajante = new Viajante();
								Viajante.Viagens = new List<Viagem>();
//								Viagem.Reservas = new List<Reserva>();
								last_viajante = item["PASSAGEIRO"].Value.toString2();
								Viajante.Passageiro = last_viajante;
								Viajante.CPF = item["CPF"].Value.toString2();
							}

							if (last_CC != item["CC_CLIENTE"].Value.toString2())
							{
								response.CentrosCusto.Add(CentroCusto);
								CentroCusto = new CentroCusto();
								CentroCusto.Viajantes = new List<Viajante>();
								last_CC = item["CC_CLIENTE"].Value.toString2();
								CentroCusto.Nome = last_CC;
								CentroCusto.Cliente = item["CLIENTE"].Value.toString2();
							}


							if (first_data == null_data) first_data = item["DATA_ORIGEM"].Value.toDataTime();

							if (Viagem.Origem.Aeroporto == null) Viagem.Origem = Reserva.Origem;

							if(Reserva.Produto == "Hotel") Viagem.Destino = Reserva.Destino;

							last_data = item["DATA_DESTINO"].Value.toDataTime();

//							CentroCusto.Viajantes.Add(Viajante);
						}
					}
				}
				debug = "010";	*/

				return response;
			}
			catch (Exception ex)
			{
				Logger.LogDetail(string.Format("Erro ao consultar adiantamento. Erro: {0}", ex.Message));
				throw new Exception(string.Format("Erro ao consultar adiantamento. Erro: {0} (Linha:{1})", ex.Message, debug));
			}
			finally
			{
				Logger.LogDetail("Método ConsultarAdiantamento() - Out");
			}
		}
	}
}