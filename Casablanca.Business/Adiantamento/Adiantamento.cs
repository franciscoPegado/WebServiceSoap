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
					Query query = new Query(@"
						DECLARE @DATAINICIO DATETIME
						SET @DATAINICIO = :PDATAINICIAL

						DECLARE @DATAFINAL DATETIME
						SET @DATAFINAL = :PDATAFINAL

						DECLARE @SITUACOES TABLE(HANDLE int, SITUACAO varchar(15))
						INSERT INTO @SITUACOES (HANDLE, SITUACAO) values (1, 'Ativo'), (2, 'Cancelado')

						DECLARE @PRODUTOS TABLE(HANDLE int, PRODUTO varchar(15), PESO int)
						INSERT INTO @PRODUTOS (HANDLE, PRODUTO, PESO) values 
							(1, 'Aéreo', 2),
							(2, 'Hotel', 3),
							(3, 'Carro', 4),
							(4, 'Miscelaneo', 1),
							(5, 'Aéreo', 2),
							(6, 'Aéreo', 2),
							(7, 'Aéreo', 2),
							(8, 'Rodoviário', 2)

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

						SELECT DISTINCT
							A.HANDLE,
							SIT.SITUACAO,
							PNR.REEMISSAO,
							CONVERT(DATE, A.DATAEMISSAO) AS DATAEMISSAO,
							A.AGENTE_EMISSAO AS EMISSOR_ID,
							AGE.NOME AS EMISSOR_NOME,
							PNR.BB_RLOC AS RLOC,
							PNR.LOCALIZADORACIA AS LOC,
							A.INFAPROVADOR AS APROVADOR,
							ISNULL(CONS.NOME, '') AS CONSOLIDADORA,
							CONVERT(VARCHAR(50), FORN.NOME) AS FORNECEDOR,
							ISNULL(A.BILHETE, '') AS BILHETE,
							ISNULL(A.CONFIRMACAO, '') AS CONFIRMACAO,
						--	ISNULL(TEMI.TIPOEMISSAO, '') AS TIPODEEMISSAO,
							CONVERT(VARCHAR(50), CLI.NOME) AS CLIENTE,
							ISNULL(TVOO.TIPOVOO, '')  AS TIPOVOO,
							ISNULL(PASS.NOME, A.PASSAGEIRONAOCAD) AS PASSAGEIRO,
							ISNULL(PASS.CPF, 'Não cadastrado') AS CPF,
							CASE A.PRODUTO WHEN 4 THEN CASE TIPMIS.HANDLE WHEN 1 THEN 'Seguro' ELSE 'Miscelanio' END ELSE PRO.PRODUTO END AS PRODUTO,
							TIPMIS.NOME AS MISCELANIO,
							ISNULL(A.TRECHOS, '') AS TRECHOS,
							CONVERT(DATETIME, ISNULL(TRE.BB_AEREO_DATA_VOO, ISNULL(A.BB_MISC_ENTRADA, ISNULL(A.BB_CARRO_RETIRADA, A.BB_HOTEL_ENTRADA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '00:00:00')) AS DATA_ORIGEM,
							ISNULL(TRE.BB_AEREO_ORIGEM, '') AS AEREO_ORIGEM,
							ISNULL(ORI_MUN.NOME, '') AS MUNICIPIO_ORIGEM,
							ISNULL(CASE ORI_PAIS.NOME WHEN 'Brasil' THEN ORI_EST.NOME ELSE NULL END, '') AS ESTADO_ORIGEM,
							ISNULL(CASE ORI_PAIS.NOME WHEN 'Brasil' THEN ORI_REG.NOME ELSE NULL END, '') AS REGIAO_ORIGEM,
							ISNULL(ORI_PAIS.NOME, '') AS PAIS_ORIGEM,
							ISNULL(CASE ORI_PAIS.NOME WHEN 'Brasil' THEN 'América Do Sul' ELSE ORI_REG.NOME END, '') AS CONTINENTE_ORIGEM,
							CONVERT(DATETIME, ISNULL((CASE WHEN CONVERT(DATETIME, TRE.BB_AEREO_DATA_VOO + TRE.BB_AEREO_HRSAIDA) > CONVERT(DATETIME, TRE.BB_AEREO_DATA_VOO + TRE.BB_AEREO_HRCHEGADA) THEN DATEADD(DAY, 1, TRE.BB_AEREO_DATA_VOO) ELSE TRE.BB_AEREO_DATA_VOO END), ISNULL(A.BB_MISC_SAIDA, ISNULL(A.BB_CARRO_ENTREGA, A.BB_HOTEL_SAIDA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '')) AS DATA_DESTINO,
							ISNULL(ISNULL(TRE.BB_AEREO_DESTINO, DES_MUN.SIGLA), '') AS AEREO_DESTINO,
							ISNULL(DES_MUN.NOME, '') AS MUNICIPIO_DESTINO,
							ISNULL(CASE DES_PAIS.NOME WHEN 'Brasil' THEN DES_EST.NOME ELSE NULL END, '') AS ESTADO_DESTINO,
							ISNULL(CASE DES_PAIS.NOME WHEN 'Brasil' THEN DES_REG.NOME ELSE NULL END, '') AS REGIAO_DESTINO,
							ISNULL(DES_PAIS.NOME, '') AS PAIS_DESTINO,
							ISNULL(CASE DES_PAIS.NOME WHEN 'Brasil' THEN 'América Do Sul' ELSE DES_REG.NOME END, '') AS CONTINENTE_DESTINO,
							FPAG.FORMAPAGAMENTO AS FORMADEPAGAMENTO,
							FREC.FORMARECEBIMENTO AS FORMARECEBIMENTO,
							PCCAGE.PCC AS PCC_EMISSAO,
							CC.NOME AS CC_CASABLANCA,
							CLI_CC.CENTROCUSTO AS CC_CLIENTE,
							CASE WHEN A.PRODUTO <> 4 OR TIPMIS.HANDLE = 1 THEN PRO.PESO ELSE 5 END AS PESO,
							CONVERT(DATE, ISNULL(TRE.BB_AEREO_DATA_VOO, ISNULL(A.BB_MISC_ENTRADA, ISNULL(A.BB_CARRO_RETIRADA, A.BB_HOTEL_ENTRADA))) + ' ' + ISNULL(TRE.BB_AEREO_HRSAIDA, '00:00:00')) AS DT_ORIGEM
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
							LEFT JOIN (SELECT MAX(NOME) AS NOME, PESSOA, CPF FROM GN_PESSOACONTATOS GROUP BY CPF, PESSOA) PASS ON PASS.PESSOA = CLI.HANDLE AND PASS.NOME LIKE REPLACE(CASE WHEN 
								CHARINDEX('/', A.PASSAGEIRONAOCAD) = 0 
								THEN A.PASSAGEIRONAOCAD ELSE 
								RIGHT(A.PASSAGEIRONAOCAD, LEN(A.PASSAGEIRONAOCAD) - CHARINDEX('/', A.PASSAGEIRONAOCAD))
								+ ' ' + 
								LEFT(A.PASSAGEIRONAOCAD, CHARINDEX('/', A.PASSAGEIRONAOCAD) - 1)
								END, ' ', '%')
						--	LEFT JOIN BB_PNRPASSAGEIROS AS PASS ON A.PASSAGEIROCAD = PASS.HANDLE
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
						ORDER BY CC_CLIENTE, PASSAGEIRO, DT_ORIGEM, PESO, RLOC, DATA_ORIGEM");

					query.Parameters.Add(new Parameter("PDATAINICIAL", DataType.DateTime, request.DataInicial));
					query.Parameters.Add(new Parameter("PDATAFINAL", DataType.DateTime, request.DataFinal));

					Logger.LogDetail(string.Format("Comando utilizado para consulta de viagens: {0}", query.CommandText));
					Entities<EntityBase> dados = query.Execute();
					if (dados.Count == 0)
						response.Excecoes.Add("Nenhuma viagem encontrada com o filtro informado!");
					else
					{
						debug = "Leiu os dados";

//						COMO LER OS VOOS:
//						OS TRECHOS PODEM APARECER ASSIM:
//						FLN/GRU/DFW/SFO/DFW/GRU/REC ONDE SFO É O DESTINO
//						REC/GRU/IAH/SFO/EWR/GRU/REC ONDE EWR É O DESTINO
//						REC/GIG/ATL/SFO-YYZ/JFK/GRU/REC ONDE SFO É O DESTINO
//						REC/BEL e BEL/BSB/REC ONDE BEL É O DESTINO

//						PRIMEIRA ENTRADA ABRE A VIAGEM
//						COM A VIAGEM AINDA FECHADA:
//							PRODUTO = SEGURO -> VIAGEM É A ABRANGENCIA DO SEGURO -> DURAÇÃO DA VIAGEM -> DATA INICIO -> DATA CHEGADA
//							PRODUTO = AÉREO -> CONFIRMAR SE É A PRIMEIRA VIAGEM OU A ÚLTIMA E PEGAR OS TRECHOS COMO COLEÇÃO DE STRING PARA COMPARAR -> PRIMEIRO O ULTIMO VIAGEM, PEGAR AS DATAS LIMITE DA VIAGEM E SOBREPOR AO SEGURO -> RECALCULAR AS DIARIAS
//							PRODUTO = HOTEL -> SE NÃO TEM AEREO E NEM SEGURO PEGAR AS DIARIAS E AS DATAS DE INICIO E FIM DA VIAGEM E COMPARAR COM OS TRECHOS PARA DEFINIR O PRIMEIRO DESTINO
//							
//	VIAGEM ─────────────────────────────────────┐	DATA DE INICIO DA VIAGEM - Ainda não tem viagem, ou mudaram os trechos, ou foi feito o último trecho, ou apareceu outro seguro, ou apareceu um hotel fora da rota
//	│┌─ SEGURO						─┐			│	Abre viagem - Data Origem - Diarias - HaSeguro
//	││┌─ VOO IDA					 │			│	Trechos - Trecho coincide com o inicio dos trechos
//	│││  └─ TRECHOS [FOR-CGH-MIA]	 │			│	Abre viagem - Data Origem - HaAereo = PRECISSA DOS TRECHOS OU DE SEGURO OU DE HOTEL PARA SER LOCALIZADO
//	│││┌─ HOTEL [MIA]				 │ DIARIAS	│	Abre viagem - Data Origem - Data Destino - Diarias - HaHotel
//	││││  -> PASSEIOS				 │ EM MIA	│
//	││││  -> INGRESSOS				 │			│
//	││││  -> CARRO, ETC...			 │			│
//	│││└─							─┘			│
//	│││					LOOP MULTIPLES DESTINOS	│
//	││├─ DESLOCAMENTO				─┐			│	Tem um hotel na cidade do trecho ou a diferência com o voo anterior é superior a 6 horas
//	│││  └─ TRECHOS [MIA-ORL]		 │			│	Data Destino - Diarias - Fecha destino - Abre destino novo - Data Origem - HaAereo = PRECISSA DOS TRECHOS OU DE SEGURO OU DE HOTEL PARA SER LOCALIZADO
//	│││┌─ HOTEL [ORL]				 │ DIARIAS	│	Abre destino novo - Data Origem - Data Destino - Diarias - HaHotel
//	││││  -> PASSEIOS				 │ EM ORL	│
//	││││  -> INGRESSOS				 │			│
//	││││  -> CARRO, ETC...			 │			│
//	│││└─							 │			│
//	│││			FIM DO LOOP MULTIPLES DESTINOS	│
//	││└─ VOO RETORNO				 │			│	Trecho coincide com o final dos trechos
//	││   └─ TRECHOS [ORL-MEX-REC]	 │			│	Data Destino - Fecha destino - Diarias = PRECISSA DOS TRECHOS OU DE SEGURO OU DE HOTEL PARA SER LOCALIZADO
//	│└─								─┘			│
//	└───────────────────────────────────────────┘	DATA DE FIM DA VIAGEM

//	primeiro ler as vendas feitas para o cliente
//	identificar o tipo de produto
//	se o produto for aéreo, pegar as datas e horas das viagens para saber nos trechos qual foi o destino



//	INFORMAÇÕES DO CENTRO DE CUSTO:
//		- NOME
//	INFORMAÇÕES DO PAX:
//		- NOME COMPLETO
//		- NOME PAX
//		- CPF
//		- ENDERECO
//		- TELEFONE
//		- EMAIL
//	INFORMAÇÕES DA VIAGEM:
//		- DATA INICIO
//		- DATA FIM
//		- DIARIAS
//		- DESTINOS
//			- DATA CHEGADA
//			- DATA RETORNO
//			- LUGAR
//			- RESERVAS
//				- DADOS DE CADA RESERVA - VOOS APENAS OS DE IDA ATÉ O ÚLTIMO VIAGEM
//		
						// VARIAVES DE COMPARAÇÃO
//						string last_rloc = null;
//						int last_rloc_pos = 0;
//						string last_origem = null;
//						bool tem_hotel = false;

//						CONFERIR SE É HOTEL.
//						SE É, MARCAR O DESTINO CONFERIR AS RESERVAS ANTERIORES
//						ONDE ENCONTRAR O MESMO DESTINO ADICIONAR NAS RESERVAS.
//							ONDE FOR AVIÃO, PEGAR O RLOC
//								ONDE FOR AVIÃO OU COM O MESMO RLOC, ADICIONAR NAS RESERVAS
//						COLOCAR INICIO DA VIAGEM NO PRIMEIRO ORIGEM
//						COLOCAR AS DIARIAS A DIFERENCIA ENTRE O PRIMEIRO ORIGEM E O ÚLTIMO DESTINO
//						Tem que ser tempo tudo o mesmo viajante e o mesmo centro de custo
//------------------------------------------------------------------------------
						response.CentrosCusto = new List<CentroCusto>();
						ConsultaAdiantamentoResponse Adiantamento;
						CentroCusto CentroCusto;
						Viajante Viajante;
						Viagem Viagem;
						Destino Destino;
						Reserva Reserva;
						ItemProduto NovoItem;
						ItemProduto LastItem;

						//	INICIALIZAÇÕES
						Adiantamento = new ConsultaAdiantamentoResponse();
						Adiantamento.CentrosCusto = new List<DataContracts.Adiantamento.CentroCusto>();
						CentroCusto = new CentroCusto();
						CentroCusto.Viajantes = new List<DataContracts.Adiantamento.Viajante>();
						Viajante = new Viajante();
						Viajante.Viagens = new List<DataContracts.Adiantamento.Viagem>();
						Viagem = new Viagem();
						Viagem.Destinos = new List<DataContracts.Adiantamento.Destino>();
						Destino = new Destino();
						Destino.Reservas = new List<DataContracts.Adiantamento.Reserva>();
						Reserva = new Reserva();
//						Reserva.Seguro = new ItemProduto();
						Reserva.AéreoIda = new List<ItemProduto>();
//						Reserva.Hotel = new ItemProduto();
//						Reserva.Carro = new ItemProduto();
						Reserva.Miscelanio = new List<ItemProduto>();
						Reserva.AéreoVolta = new List<ItemProduto>();
						


						//	TESTEMUNHAS
						bool novoReserva = true;
						bool novoDestino = true;
						bool novoViagem = true;
						bool novoViajante = true;
						bool novoCentroCusto = true;
						bool inicio = true;
						bool ida = true;

						DateTime null_data = new System.DateTime(2000, 1, 1, 0, 0, 0);	// Isto vai ser equivalente a datetime nula
						DateTime first_data = null_data;								// Aquí é para gerar uma variavel datetime nula
						DateTime last_data = null_data;									// Aquí é para gerar uma variavel datetime nula

						foreach (EntityBase item in dados)
						{
							debug = "LÉ O ITEM";

							NovoItem = new ItemProduto();
							NovoItem.TrOrigem = new Endereco();
							NovoItem.TrDestino = new Endereco();
							NovoItem.TipoProduto = item["PRODUTO"].Value.toString2();
							NovoItem.CodigoCasablanca = item["HANDLE"].Value.toString2();
							NovoItem.Loc = item["LOC"].Value.toString2();
							NovoItem.Aprovador = item["APROVADOR"].Value.toString2();
							NovoItem.Fornecedor = item["FORNECEDOR"].Value.toString2();
							NovoItem.Inicio = item["DATA_ORIGEM"].Value.toDataTime();
							NovoItem.Final = item["DATA_DESTINO"].Value.toDataTime();
							NovoItem.Diarias = NovoItem.Final.Subtract(NovoItem.Inicio).Days + 1;

							switch (NovoItem.TipoProduto)
							{
								case "Seguro":
									NovoItem.Confirmacao = item["CONFIRMACAO"].Value.toString2();

									NovoItem.TrOrigem = null;

									NovoItem.TrDestino = new Endereco();
									NovoItem.TrDestino.Municipio = item["MUNICIPIO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Estado = item["ESTADO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Regiao = item["REGIAO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Pais = item["PAIS_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Continente = item["CONTINENTE_DESTINO"].Value.toString2();
									if (Viagem.Seguro != null) novoViagem = true;		// Seguro repetido = novo viagem
									break;

								case "Aéreo":
									NovoItem.Bilhete = item["BILHETE"].Value.toString2();

									NovoItem.TrOrigem = new Endereco();
									NovoItem.TrOrigem.Aeroporto = item["AEREO_ORIGEM"].Value.toString2();
									NovoItem.TrOrigem.Municipio = item["MUNICIPIO_ORIGEM"].Value.toString2();
									NovoItem.TrOrigem.Estado = item["ESTADO_ORIGEM"].Value.toString2();
									NovoItem.TrOrigem.Regiao = item["REGIAO_ORIGEM"].Value.toString2();
									NovoItem.TrOrigem.Pais = item["PAIS_ORIGEM"].Value.toString2();
									NovoItem.TrOrigem.Continente = item["CONTINENTE_ORIGEM"].Value.toString2();

									NovoItem.TrDestino = new Endereco();
									NovoItem.TrDestino.Aeroporto = item["AEREO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Municipio = item["MUNICIPIO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Estado = item["ESTADO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Regiao = item["REGIAO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Pais = item["PAIS_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Continente = item["CONTINENTE_DESTINO"].Value.toString2();
									break;

								case "Hotel":
									NovoItem.Confirmacao = item["CONFIRMACAO"].Value.toString2();

									NovoItem.TrOrigem = null;

									NovoItem.TrDestino = new Endereco();
									NovoItem.TrDestino.Municipio = item["MUNICIPIO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Estado = item["ESTADO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Regiao = item["REGIAO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Pais = item["PAIS_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Continente = item["CONTINENTE_DESTINO"].Value.toString2();
									break;

									if (Reserva.Hotel != null)
									{
										if (Reserva.Hotel.Final.Subtract(NovoItem.Inicio).Days > 1)
										{
											novoViagem = true;
										}
										else
										{
											novoDestino = true;
										}
									}

								case "Carro":
									NovoItem.Confirmacao = item["CONFIRMACAO"].Value.toString2();

									NovoItem.TrOrigem = null;

									NovoItem.TrDestino = new Endereco();
									NovoItem.TrDestino.Municipio = item["MUNICIPIO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Estado = item["ESTADO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Regiao = item["REGIAO_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Pais = item["PAIS_DESTINO"].Value.toString2();
									NovoItem.TrDestino.Continente = item["CONTINENTE_DESTINO"].Value.toString2();
									break;

								case "Miscelanio":
//									Reserva.Miscelanio.Add(NovoItem);
									break;	// Miscelanio morre aqui.
							}

							debug = "DECIDE SE VAI MUDAR O DESTINO OU VIAGEM OU VIAJANTE OU CENTRO DE CUSTO";

							//	primeira comporbação:
							if (Viajante.CPF != item["CPF"].Value.toString2() || Viajante.Nome != item["PASSAGEIRO"].Value.toString2()) novoViajante = true;
							if (CentroCusto.Nome != item["CC_CLIENTE"].Value.toString2()) novoCentroCusto = true;

							debug = "FECHA E ABRE O QUE SEJA NECESSARIO";

							if (novoReserva || novoDestino || novoViagem || novoViajante || novoCentroCusto)
							{	// NOVA RESERVA
								debug = "NOVA RESERVA";
								if (!inicio)
								{
									Destino.Reservas.Add(Reserva);
									Reserva = new Reserva();
//									Reserva.Seguro = new ItemProduto();
									Reserva.AéreoIda = new List<ItemProduto>();
//									Reserva.Hotel = new ItemProduto();
//									Reserva.Carro = new ItemProduto();
									Reserva.Miscelanio = new List<ItemProduto>();
									Reserva.AéreoVolta = new List<ItemProduto>();
								}
							}

							//	DECISSÕES PARA NOVO DESTINO

							if (novoDestino || novoViagem || novoViajante || novoCentroCusto)
							{	// NOVO DESTINO
								debug = "NOVO DESTINO";
								if (!inicio)
								{
									Viagem.Destinos.Add(Destino);
									Destino = new Destino();
									Destino.Reservas = new List<DataContracts.Adiantamento.Reserva>();
									ida = true;
								}
							}

							//	DECISSÕES PARA NOVA VIAGEM

							if (novoViagem || novoViajante || novoCentroCusto)
							{	// NOVO VIAGEM
								debug = "NOVO VIAGEM";
								if (!inicio)
								{
									Viajante.Viagens.Add(Viagem);
									Viagem = new Viagem();
									Viagem.Destinos = new List<DataContracts.Adiantamento.Destino>();
								}
							}

							if (novoViajante || novoCentroCusto)
							{	// NOVO VIAJANTE
								debug = "NOVO VIAJANTE";
								if (!inicio)
								{
									CentroCusto.Viajantes.Add(Viajante);
									Viajante = new Viajante();
									Viajante.Viagens = new List<DataContracts.Adiantamento.Viagem>();
								}
								Viajante.Nome = item["PASSAGEIRO"].Value.toString2();
								Viajante.CPF = item["CPF"].Value.toString2();
							}

							if (novoCentroCusto)
							{	// NOVO CENTRO DE CUSTO
								debug = "NOVO CENTRO DE CUSTO";
								if (!inicio)
								{
									response.CentrosCusto.Add(CentroCusto);
									CentroCusto = new CentroCusto();
									CentroCusto.Viajantes = new List<DataContracts.Adiantamento.Viajante>();
								}
								CentroCusto.Nome = item["CC_CLIENTE"].Value.toString2();
								CentroCusto.Cliente = item["CLIENTE"].Value.toString2();
							}

							inicio = false;
							novoReserva = false;
							novoDestino = false;
							novoViagem = false;
							novoViajante = false;
							novoCentroCusto = false;

							debug = "COLOCA O ITEM";
							if(Viagem.Trechos == null) {
								Viagem.Trechos = item["TRECHOS"].Value.toString2();
							} else {
								if(Viagem.Trechos != item["TRECHOS"].Value.toString2()) {
									Viagem.Trechos += item["TRECHOS"].Value.toString2();
								}
							}

							if (Destino.Diarias < NovoItem.Diarias)
							{
								Viagem.Diarias += NovoItem.Diarias - Destino.Diarias;
								Destino.Diarias = NovoItem.Diarias;
							}

							switch (NovoItem.TipoProduto)
							{
								case "Seguro":
									Viagem.Seguro = NovoItem;
									Viagem.Inicio = NovoItem.Inicio;
									Viagem.Final = NovoItem.Final;
									break;

								case "Aéreo":	//Decidir se é ida ou volta
									debug = "AEREO " + Reserva.AéreoIda.Count();
									if(ida && Reserva.AéreoIda.Count() > 0){
										if (NovoItem.Inicio.Subtract(Reserva.AéreoIda.Last().Final).Days > 1 && NovoItem.TrOrigem.Municipio == Reserva.AéreoIda.Last().TrDestino.Municipio)
										{
											Destino.TrDestino = NovoItem.TrOrigem;
											ida = false;
										}
									}
									if (ida)			//integerList[integerList.Count - 1]
									{
										if (Destino.TrOrigem == null) Destino.TrOrigem = NovoItem.TrOrigem;
										Reserva.AéreoIda.Add(NovoItem);
									} else {
										Reserva.AéreoVolta.Add(NovoItem);
									}
									break;

								case "Hotel":
									Reserva.Hotel = NovoItem;
									Destino.TrDestino = NovoItem.TrDestino;
									ida = false;
									break;

								case "Carro":
									Reserva.Carro = NovoItem;
									if (Destino.TrDestino == null)
									{
										Destino.TrDestino = NovoItem.TrDestino;
									}
									ida = false;
									break;

								case "Miscelanio":
									Reserva.Miscelanio.Add(NovoItem);
									break;
							}

							debug = "LINHA 02";
							LastItem = NovoItem;
						}
						Destino.Reservas.Add(Reserva);
						Viagem.Destinos.Add(Destino);
						Viajante.Viagens.Add(Viagem);
						CentroCusto.Viajantes.Add(Viajante);
						response.CentrosCusto.Add(CentroCusto);
					}
				}
				debug = "Acabou a revissão";

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