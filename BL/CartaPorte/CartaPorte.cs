using ML.CartaPorte;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BL.CartaPorte
{
    public class CartaPorte
    {
        public static ML.Result GetLoadInfo(ML.CartaPorte.QueryCarta queryCarta, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using(OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
				{
                    connection.Open();

					ML.Result resultValidateLoad = ValidateLoad(connection, queryCarta);
					if (!resultValidateLoad.Correct)
					{
						throw new Exception($@"{resultValidateLoad.Message}");
					}
					bool exist = (bool) resultValidateLoad.Object;

					if (!exist)
					{
						throw new Exception($@"No se encontró la carga {queryCarta.CarSal} registrada");
					}
					else
					{
                        ML.Result resultValidateExist = ValidateExist(connection, queryCarta.PtoAlm, queryCarta.CarSal);
                        if (!resultValidateExist.Correct)
                        {
                            throw new Exception($@"{resultValidateExist.Message}");
                        }
                        string idUni = (string)resultValidateExist.Object;
                        
                        if(idUni != string.Empty)
                        {
                            throw new Exception($@"Ya existe la carta con el id {idUni}");
                        }

                        ML.Result resultGetScnDir = GetScnDir(connection, queryCarta);
                        if (!resultGetScnDir.Correct)
                        {
                            throw new Exception($@"{resultGetScnDir.Message}");
                        }
                        List<ML.CartaPorte.ScnDir> scnDirList = (List<ML.CartaPorte.ScnDir>)resultGetScnDir.Object;

						result.Correct = true;
						result.Object = scnDirList;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al recuperar Info de la carga {ex.Message}";
            }
            return result;
        }
        private static ML.Result ValidateLoad(OdbcConnection connection, ML.CartaPorte.QueryCarta queryCarta)
        {
            ML.Result result = new ML.Result();
            try
            {
                string query = $@"SELECT COUNT(*)
									FROM ora_ruta
									WHERE pto_alm = ?
									AND car_sal = ?
									";
				bool exist = false;

				using(OdbcCommand cmd =  new OdbcCommand(query, connection))
				{
					cmd.Parameters.AddWithValue("pto_alm", queryCarta.PtoAlm);
					cmd.Parameters.AddWithValue("car_sal", queryCarta.CarSal);

					using(OdbcDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							exist = reader.GetInt32(0) > 0 ? true : false;
						}
					}
				}

				result.Correct = true;
				result.Object = exist;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al validar {ex.Message}";
            }
            return result;
        }
        private static ML.Result ValidateExist(OdbcConnection connection, string ptoAlm, string carSal)
        {
            ML.Result result = new ML.Result();
            try
            {
                string query = $@"SELECT id_uni
                                    FROM cartaporte_tracking
                                    WHERE pto_alm = ?
                                    AND car_sal = ?
									";
                bool exist = false;
                string idUni = string.Empty;

                using (OdbcCommand cmd = new OdbcCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("pto_alm", ptoAlm);
                    cmd.Parameters.AddWithValue("car_sal", carSal);

                    using (OdbcDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            //exist = reader.GetInt32(0) > 0 ? true : false;
                            idUni = reader.GetString(0);
                        }
                    }
                }

                result.Correct = true;
                result.Object = idUni;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al validar si ya tiene cartaPorte {ex.Message}";
            }
            return result;
        }
        private static ML.Result GetScnDir(OdbcConnection connection, ML.CartaPorte.QueryCarta queryCarta)
        {
            ML.Result result = new ML.Result();
            try
            {
                string query = $@"SELECT 
									A.num_scn,
									B.cod_dir,
									CASE
									WHEN(G.dir_cli IS NOT NULL)
									THEN TRIM(G.dir_cli)
									ELSE 'N/A'
									END AS dir_cli,
									CASE
									WHEN (G.num_ext IS NOT NULL AND TRIM(G.num_ext) <> '')
									THEN TRIM(G.num_ext)
									ELSE '0'
									END AS num_ext,
									CASE
									WHEN (G.num_int IS NOT NULL AND TRIM(G.num_int) <> '')
									THEN TRIM(G.num_int)
									ELSE '0'
									END AS num_int,
									CASE WHEN(H.cve_col IS NOT NULL)
									THEN 1
									ELSE 0
									END AS ok_col,
									CASE WHEN(H.cve_col IS NOT NULL)
									THEN H.cve_col
									ELSE '0'
									END AS cve_col,
									TRIM(G.col_cli) AS col_cli,
									CASE WHEN(J.cve_mun IS NOT NULL)
									THEN 1
									ELSE 0
									END AS ok_mun,
									CASE WHEN(J.cve_mun IS NOT NULL)
									THEN J.cve_mun
									ELSE '0'
									END AS cve_mun,
									TRIM (G.pob_cli) AS mun_cli,
									CASE WHEN(I.cve_est IS NOT NULL)
									THEN 1
									ELSE 0
									END AS ok_est,
									CASE WHEN(I.cve_est IS NOT NULL)
									THEN I.cve_est
									ELSE 'XXX'
									END AS cve_est,
									TRIM(G.pro_cli) AS pro_cli,
									CASE WHEN(H.cod_postal IS NOT NULL)
									THEN 1
									ELSE 0
									END AS ok_cp,
									CASE WHEN(H.cod_postal IS NOT NULL)
									THEN H.cod_postal
									ELSE 'XXXXX'
									END AS cve_cp,
									TRIM(G.cp_cli) AS cp_cli
								FROM ora_ruta A
								INNER JOIN edc_cab B
									 ON B.cod_emp = 1
									AND B.num_scn = A.num_scn
								INNER JOIN cli_direccion G
									 ON G.cod_emp = B.cod_emp
									AND G.cod_dir = B.cod_dir
								LEFT JOIN cat_cp H
									 ON H.cod_postal = G.cp_cli
									AND TRIM(LOWER(H.nom_col)) = TRIM(LOWER(G.col_cli))
								LEFT JOIN cat_est I
									 ON I.pais = 'MEX'
									AND I.cve_est = G.pro_cli
								LEFT JOIN cat_mun J
									 ON J.cve_est = I.cve_est
									AND LOWER(J.desc) = LOWER(G.pob_cli)
								WHERE A.pto_alm = ?
								AND A.car_sal = ?
								ORDER BY B.cod_dir";

				List<ML.CartaPorte.ScnDir> scnDirList = new List<ML.CartaPorte.ScnDir>();

				using(OdbcCommand cmd =  new OdbcCommand(query, connection))
				{
                    cmd.Parameters.AddWithValue("pto_alm", queryCarta.PtoAlm);
                    cmd.Parameters.AddWithValue("car_sal", queryCarta.CarSal);

                    using (OdbcDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
                            ML.CartaPorte.ScnDir scnDir = new ML.CartaPorte.ScnDir();

                            scnDir.NumScn = reader.GetString(0);
                            scnDir.CodDir = reader.GetString(1);
                            scnDir.Calle = reader.GetString(2);
                            scnDir.NumExt = reader.GetString(3);
                            scnDir.NumInt = reader.GetString(4);
                            scnDir.OkCol = reader.GetInt32(5) == 1 ? true : false;
                            scnDir.CveCol = reader.GetString(6);
                            scnDir.ColCli = reader.GetString(7);
                            scnDir.OkMun = reader.GetInt32(8) == 1 ? true : false;
                            scnDir.CveMun = reader.GetString(9);
                            scnDir.MunCli = reader.GetString(10);
                            scnDir.OkEst = reader.GetInt32(11) == 1 ? true : false;
                            scnDir.CveEst = reader.GetString(12);
                            scnDir.EstCli = reader.GetString(13);
                            scnDir.OkCp = reader.GetInt32(14) == 1 ? true : false;
                            scnDir.CveCp = reader.GetString(15);
                            scnDir.CpCli = reader.GetString(16);

                            scnDir.OkDireccion =	scnDir.OkCol &&
													scnDir.OkMun &&
													scnDir.OkEst &&
													scnDir.OkCp;


                            scnDirList.Add(scnDir);
                        }
					}
				}

				if(scnDirList.Count < 1)
				{
					throw new Exception($@"No se leyó dato alguno.");
				}

                foreach (ScnDir x in scnDirList)
                {
                    if (string.IsNullOrWhiteSpace(x.Calle))
                        continue;

                    x.Calle = Regex.Replace(
                                    x.Calle
                                        .Replace("\t", " ")
                                        .Replace("|", " ")
                                        .Replace(",", " ")
                                        .Replace("ñ", "n")
                                        .Replace("Ñ", "N"),
                                    @"\s+",
                                    " ")
                                .Trim();
                }


                result.Correct = true;
				result.Object = scnDirList;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al recuperar SCNs {ex.Message}";
            }
            return result;
        }
        /*Mantenimiento*/
        public static ML.Result GetColByCodPos(string codPos, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                ML.CartaPorte.DireccionInfo direccionInfo = new ML.CartaPorte.DireccionInfo();

                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    string queryEstado = $@"SELECT TRIM(B.cve_est),TRIM(B.nom_est)
                                    FROM cat_est2 A, cat_est B
                                    WHERE A.cod_postal = '{codPos}'
                                    AND B.cve_est = A.cve_est";

                    using (OdbcCommand cmd = new OdbcCommand(queryEstado, connection))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                direccionInfo.CveEstado = reader.GetString(0);
                                direccionInfo.Estado = reader.GetString(1);
                            }
                        }
                    }

                    string queryMunicipio = $@"SELECT cve_mun,TRIM(desc)
                                                FROM cat_mun2
                                                WHERE cod_postal = '{codPos}'";

                    using (OdbcCommand cmd = new OdbcCommand(queryMunicipio, connection))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                direccionInfo.CveMunicipio = reader.GetString(0);
                                direccionInfo.Municipio = reader.GetString(1);
                            }
                        }
                    }

                    string queryColonias = $@"SELECT TRIM(nom_col)
                                            FROM cat_cp
                                            WHERE cod_postal = '{codPos}'";

                    using (OdbcCommand cmd = new OdbcCommand(queryColonias, connection))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            direccionInfo.Colonias = new List<string>();

                            while (reader.Read())
                            {
                                string col = reader.GetString(0);

                                direccionInfo.Colonias.Add(col);
                            }
                        }
                    }
                }


                result.Correct = true;
                result.Object = direccionInfo;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Ex = ex;
                System.Console.WriteLine($@"{ex.Message}");
            }
            return result;
        }
        public static ML.Result Maintenance(ML.CartaPorte.ScnDir scnDir, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                /*
                    ColCli  col_cli
                    MunCli  pob_cli
                    CveEst  pro_cli
                    CveCp   cp_cli
                 */

                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    string query = $@"UPDATE
                                             cli_direccion
                                        SET     col_cli = ? ,
                                                pob_cli = ? ,
                                                pro_cli = ? ,
                                                cp_cli = ?
                                        WHERE cod_emp = 1
                                        AND cod_dir = ?
                                        ";

                    using(OdbcCommand cmd = new OdbcCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("col_cli", scnDir.ColCli);
                        cmd.Parameters.AddWithValue("pob_cli", scnDir.MunCli);
                        cmd.Parameters.AddWithValue("pro_cli", scnDir.CveEst);
                        cmd.Parameters.AddWithValue("cp_cli", scnDir.CveCp);
                        cmd.Parameters.AddWithValue("cod_dir", scnDir.CodDir);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected < 1) 
                        {
                            throw new Exception($@"registros afectados 0");
                        }
                    }
                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al corregir direccion {ex.Message}";
            }
            return result;
        }
        /*Obtener Choferes oper_tda*/
        public static ML.Result GetOper(string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    string query = $@"SELECT TRIM(rfc_ope) AS rfc_ope,
	                                        TRIM(lic_ope) AS lic_ope,
	                                        TRIM(nom_ope) AS nom_ope,
	                                        CASE WHEN (rfc_ope IN ('CAST900102CS9','1'))
	                                        THEN
		                                        'EXTERNO'
	                                        ELSE
		                                        'INTERNO'
	                                        END AS tipo
                                        FROM oper_tda
                                        WHERE cod_emp = 1
                                        AND cod_pto = 870
                                        ORDER BY 3
                                        ";

					List< ML.CartaPorte.OperTda> operTdaList = new List<ML.CartaPorte.OperTda>();

					using(OdbcCommand cmd = new OdbcCommand(query, connection))
					{
						using (OdbcDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								ML.CartaPorte.OperTda operTda = new ML.CartaPorte.OperTda();

								operTda.RfcOpe = reader.GetString(0).Trim();
								operTda.LicOpe = reader.GetString(1).Trim();
								operTda.NomOpe = reader.GetString(2).Trim();
								operTda.Tipo = reader.GetString(3).Trim();

                                operTdaList.Add(operTda);
                            }
						}
					}

                    result.Correct = true;
                    result.Object = operTdaList;
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al recuperar choferes {ex.Message}";
            }
            return result;
        }
        /*Obtener Transporte tra_pro*/
        public static ML.Result GetTrans(string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

					string query = $@"SELECT num_eco,
                                            TRIM(pla_vei) pla_vei,
                                            TRIM(mod_vei) mod_vei,
                                            TRIM(cod_vei) cod_vei,
                                            TRIM(num_cis) num_cis,
                                            TRIM(num_pol) num_pol,
                                            TRIM(nom_ase) nom_ase,
                                            TRIM(num_per) num_per,
                                            TRIM(per_sct) per_sct,
                                            TRIM(con_vei) con_vei
									FROM tra_pro
									WHERE cod_emp = 1
									AND cod_pto = 870
									ORDER BY num_eco ASC";

                    List<ML.CartaPorte.TraPro> traProList = new List<ML.CartaPorte.TraPro>();

                    using (OdbcCommand cmd = new OdbcCommand(query, connection))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ML.CartaPorte.TraPro traPro = new ML.CartaPorte.TraPro();

                                traPro.NumEco = reader.GetString(0);
                                traPro.PlaVei = reader.GetString(1);
                                traPro.ModVei = reader.GetString(2);
                                traPro.CodVei = reader.GetString(3);
                                traPro.NumCis = reader.GetString(4);
                                traPro.NumPol = reader.GetString(5);
                                traPro.NomAse = reader.GetString(6);
                                traPro.NumPer = reader.GetString(7);
                                traPro.PerSct = reader.GetString(8);
                                traPro.ConVei = reader.GetString(9);

                                traProList.Add(traPro);
                            }
                        }
                    }

                    result.Correct = true;
                    result.Object = traProList;
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al recuperar transportes {ex.Message}";
            }
            return result;
        }

        /*Armar*/
        public static ML.Result CartaPorteMaker(ML.CartaPorte.CartaInput cartaInput, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                string idUni = GenerateIdUni(cartaInput.PtoAlm);

                ML.Result resultBuildDataCarta = BuildDataCarta(cartaInput, idUni, mode);
                if (!resultBuildDataCarta.Correct)
                {
                    throw new Exception($@"{resultBuildDataCarta.Message}");
                }
                ML.CartaPorte.CartaPorte cartaPorte = (ML.CartaPorte.CartaPorte)resultBuildDataCarta.Object;

                //Insertar la carta

                if (cartaInput.Tipo == "INTERNO")
                {
                    ML.Result resultInsertInternalCartaPorte = InsertInternalCartaPorte(cartaPorte, "PRO");
                    if (!resultInsertInternalCartaPorte.Correct)
                    {
                        throw new Exception($@"{resultInsertInternalCartaPorte.Message}");
                    }
                }
                if (cartaInput.Tipo == "EXTERNO")
                {
                    ML.Result resultSendInfo = SendInfo(cartaPorte, cartaInput, mode);
                    if (!resultSendInfo.Correct)
                    {
                        throw new Exception($@"{resultSendInfo.Message}");
                    }
                }

                ML.Result resultInsertTracking = InsertTracking(cartaInput, idUni, mode);
                if (!resultInsertTracking.Correct)
                {
                    throw new Exception($@"{resultInsertTracking.Message}");
                }

                result.Correct = true;
                result.Message = $@"Proceso satisfactorio, se genero la cartaporte con id {idUni}";
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error en CartaPorte " + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static string GenerateIdUni(string ptoAlm)
        {
            return $@"SEAOIC{DateTime.Now.ToString("yyMMddHHmmss")}{int.Parse(ptoAlm).ToString("D4")}";
        }
        private static ML.Result BuildDataCarta(ML.CartaPorte.CartaInput cartaInput, string idUni, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    ML.Result resultValidateExist = ValidateExist(connection, cartaInput.PtoAlm, cartaInput.CarSal);
                    if (!resultValidateExist.Correct)
                    {
                        throw new Exception($@"{resultValidateExist.Message}");
                    }
                    string idUnix = (string)resultValidateExist.Object;

                    if (idUnix != string.Empty)
                    {
                        throw new Exception($@"Ya existe la carta con el id {idUni}");
                    }

                    ML.Result resultGetParadas = GetParadas(connection, cartaInput);
                    if (!resultGetParadas.Correct)
                    {
                        throw new Exception($@"{resultGetParadas.Message}");
                    }
                    List<ML.CartaPorte.Parada> paradas = (List<ML.CartaPorte.Parada>)resultGetParadas.Object;
                    resultGetParadas = null;

                    DateTime fecSal = DateTime.ParseExact(cartaInput.FecSal, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
                    ML.CartaPorte.CartaPorte cartaPorte = new ML.CartaPorte.CartaPorte();

                    //BuildDataDir
                    ML.Result resultBuildDataDir = BuildDataDir(paradas, idUni, fecSal);
                    if (!resultBuildDataDir.Correct)
                    {
                        throw new Exception($@"{resultBuildDataDir.Message}");
                    }
                    cartaPorte.ubiTim = (List<ML.CartaPorte.UbiTim2>)resultBuildDataDir.Objects[0];
                    cartaPorte.domTim = (List<ML.CartaPorte.DomTim2>)resultBuildDataDir.Objects[1];
                    resultBuildDataDir = null;

                    //BuildDataArts
                    ML.Result resultBuildDataArts = BuildDataArts(connection, cartaInput, paradas, idUni);
                    if (!resultBuildDataArts.Correct)
                    {
                        throw new Exception($@"{resultBuildDataArts.Message}");
                    }
                    cartaPorte.intTim = (List<ML.CartaPorte.IntTim24>)resultBuildDataArts.Objects[0];
                    decimal peso = (decimal)resultBuildDataArts.Objects[1];
                    resultBuildDataArts = null;


                    //BuildDataArts
                    ML.Result resultBuildMedios = BuildMedios(cartaInput, idUni, peso);
                    if (!resultBuildMedios.Correct)
                    {
                        throw new Exception($@"{resultBuildMedios.Message}");
                    }
                    cartaPorte.transTim = (ML.CartaPorte.TransTim24)resultBuildMedios.Objects[0];
                    cartaPorte.operTim = (ML.CartaPorte.OperTim2)resultBuildMedios.Objects[1];
                    resultBuildMedios = null;

                    
                    result.Correct = true;
                    result.Object = cartaPorte;
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error en armar " + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static ML.Result GetParadas(OdbcConnection connection,ML.CartaPorte.CartaInput cartaInput)
        {
            ML.Result result = new ML.Result();
            try
            {
                ML.CartaPorte.QueryCarta queryCarta = new QueryCarta
                {
                    PtoAlm = cartaInput.PtoAlm,
                    CarSal = cartaInput.CarSal
                };

                ML.Result resultGetScnDir = GetScnDir(connection, queryCarta);
                if (!resultGetScnDir.Correct)
                {
                    throw new Exception($@"{resultGetScnDir.Message}");
                }
                List<ML.CartaPorte.ScnDir> scnDirList = (List<ML.CartaPorte.ScnDir>)resultGetScnDir.Object;

                bool todasDireccionesOk = scnDirList.All(x => x.OkDireccion);

                if (!todasDireccionesOk)
                {
                    throw new Exception($@"Hay error en algun SCN validar");
                }

                List<ML.CartaPorte.Parada> stops = scnDirList
                            .GroupBy(x => x.CodDir)
                            .Select((g, index) =>
                            {
                                var first = g.First();

                                return new Parada
                                {
                                    CodDir = g.Key,
                                    Stop = index + 1,
                                    //Ori = $@"OR{index.ToString("D6")}",
                                    Rfc = "XAXX010101000",
                                    Calle = first.Calle,
                                    NumExt = first.NumExt,
                                    NumInt = first.NumInt,
                                    CveCol = first.CveCol,
                                    CveMun = first.CveMun,
                                    CveEst = first.CveEst,
                                    CveCp = first.CveCp,
                                    SCNCount = g.Select(x => x.NumScn).Distinct().Count()
                                };
                            })
                            .ToList();

                ML.Result resultGetParadaCDT = GetParadaCDT(connection, cartaInput.PtoAlm);
                if (!resultGetParadaCDT.Correct)
                {
                    throw new Exception($@"{resultGetParadaCDT.Message}");
                }
                List<ML.CartaPorte.Parada> paradasCDT = (List<ML.CartaPorte.Parada>)resultGetParadaCDT.Object;

                paradasCDT.AddRange(stops);

                for (int i = 0; i < paradasCDT.Count; i++)
                {
                    var actual = paradasCDT[i];

                    bool esPrimero = i == 0;
                    bool esUltimo = i == paradasCDT.Count - 1;

                    actual.Ori = esUltimo
                        ? null
                        : $"OR{actual.Stop.ToString("D6")}";

                    actual.Des = esPrimero
                        ? null
                        : $"DE{paradasCDT[i].Stop.ToString("D6")}";
                }


                result.Correct = true;
                result.Object = paradasCDT;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error en armar paradas " + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static ML.Result GetParadaCDT(OdbcConnection connection, string ptoAlm)
        {
            ML.Result result = new ML.Result();
            try
            {
                string query = $@"SELECT A.cv_almacen,'SOM101125UEA',A.calle,A.no_ext,A.no_int,B.cve_col,A.fax,C.cve_mun,
                                            A.edo_entfed,A.cod_pos
                                    FROM dblga@lga_prod:lgaalmacen A, cat_cp B, cat_mun C
                                    WHERE A.cv_almacen = {ptoAlm}
                                    AND A.cod_empresa = 1
                                    AND A.cd_id = {ptoAlm}
                                    AND B.cod_postal = A.cod_pos
                                    AND B.nom_col LIKE A.col_poblacion || '%'
                                    AND C.cve_est = A.edo_entfed
                                    AND C.desc LIKE A.mpio_deleg || '%'
                                    ";

                List<ML.CartaPorte.Parada> paradas = new List<ML.CartaPorte.Parada>();

                using (OdbcCommand cmd = new OdbcCommand(query, connection))
                {
                    using (OdbcDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ML.CartaPorte.Parada parada = new ML.CartaPorte.Parada();

                            parada.CodDir = reader.GetString(0);
                            parada.Stop = int.Parse(reader.GetString(0));
                            parada.Rfc = reader.GetString(1);
                            parada.Calle = reader.GetString(2);
                            parada.NumExt = reader.IsDBNull(3)? "0": reader.GetString(3);
                            parada.NumInt = reader.IsDBNull(4) ? "0" : reader.GetString(4);
                            parada.CveCol = reader.GetString(5);
                            parada.CveMun = reader.GetString(7);
                            parada.CveEst = reader.GetString(8);
                            parada.CveCp = reader.GetString(9);
                            parada.SCNCount = 0;

                            paradas.Add(parada);
                        }
                    }
                }

                result.Correct = true;
                result.Object = paradas;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al obtener la direccion de {ptoAlm} {ex.Message}";
            }
            return result;
        }
        private static ML.Result BuildDataDir(List<ML.CartaPorte.Parada> paradas ,string idUni, DateTime fecSal)
        {
            ML.Result result = new ML.Result();
            try
            {
                DateTime fecSalActual = fecSal;

                List<UbiTim2> ubiList = new List<UbiTim2>();

                for (int i = 0; i < paradas.Count; i++)
                {
                    var actual = paradas[i];

                    if (actual.Ori == null)
                        continue;

                    var paradaSig = paradas[i+1];

                    DateTime fecLle = fecSalActual.AddMinutes(90);

                    ubiList.Add(new UbiTim2
                    {
                        id_ori = actual.Ori,
                        id_des = paradaSig.Des,
                        rfc_rem = actual.Rfc,
                        rfc_des = paradas[i + 1].Rfc,
                        fec_sal = fecSalActual.ToString("yyyy-MM-dd HH:mm:ss"),
                        fec_lle = fecLle.ToString("yyyy-MM-dd HH:mm:ss"),
                        dis_rec = 3.2m,
                        id_uni = idUni,
                        cod_emp = 1m
                    });

                    fecSalActual = fecLle.AddMinutes(20);
                }

                //DomTim2
                List<DomTim2> domList = new List<DomTim2>();

                foreach (var p in paradas)
                {
                    if (p.Ori != null)
                    {
                        domList.Add(new DomTim2
                        {
                            des_ori = p.Ori,
                            calle = p.Calle,
                            num_ext = p.NumExt,
                            num_int = p.NumInt,
                            col = p.CveCol,
                            muni = p.CveMun,
                            est = p.CveEst,
                            cod_pos = p.CveCp,
                            id_uni = idUni
                        });
                    }

                    if (p.Des != null)
                    {
                        domList.Add(new DomTim2
                        {
                            des_ori = p.Des,
                            calle = p.Calle,
                            num_ext = p.NumExt,
                            num_int = p.NumInt,
                            col = p.CveCol,
                            muni = p.CveMun,
                            est = p.CveEst,
                            cod_pos = p.CveCp,
                            id_uni = idUni
                        });
                    }
                }

                result.Correct = true;
                result.Objects = new List<object>();
                result.Objects.Add(ubiList);
                result.Objects.Add(domList);
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error en armar domicilios y ubicaciones" + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static ML.Result BuildDataArts(OdbcConnection connection, ML.CartaPorte.CartaInput cartaInput, List<ML.CartaPorte.Parada> paradas,  string idUni)
        {
            ML.Result result = new ML.Result();
            try
            {
                string query = $@"SELECT B.cod_dir,
	                                        D.char_5 AS bie_tra,
	                                        TRIM(F.des_codfis) AS des_codfis,
	                                        C.uni_mov,
	                                        CASE WHEN(E.pes_caj IS NOT NULL AND E.pes_caj >0)
	                                        THEN
		                                        SUM(E.pes_caj)
	                                        ELSE
		                                        CASE WHEN(E.pes_art IS NOT NULL AND E.pes_art >0)
		                                        THEN
			                                        SUM(E.pes_art)
		                                        ELSE
			                                        1
		                                        END
	                                        END AS pes_art
                                        FROM ora_ruta A
                                        INNER JOIN edc_cab B
	                                         ON B.cod_emp = 1
	                                        AND B.num_scn = A.num_scn
                                        INNER JOIN edc_det C
	                                         ON C.cod_emp = B.cod_emp
	                                        AND C.cod_pto = B.cod_pto
	                                        AND C.num_edc = B.num_edc
                                        LEFT JOIN arti D
	                                         ON D.cod_emp = C.cod_emp
	                                        AND D.ean_art = C.ean_art
	                                        AND D.int_art = C.int_art
                                        LEFT JOIN arti_vol E
	                                         ON E.cod_emp = D.cod_emp
	                                        AND E.int_art = D.int_art
                                        LEFT JOIN cat_codfis F
	                                         ON F.cve_codfis = D.char_5
                                        WHERE A.pto_alm = {cartaInput.PtoAlm}
                                        AND A.car_sal = '{cartaInput.CarSal}'
                                        AND D.cod_fam2 <> '0187'
                                        AND F.cve_codfis <> '01010101'
                                        GROUP BY cod_dir,bie_tra,des_codfis,uni_mov,pes_art,pes_caj
                                        ORDER BY B.cod_dir";

                List<ML.CartaPorte.Art> arts = new List<ML.CartaPorte.Art>();

                using(OdbcCommand cmd = new OdbcCommand(query, connection))
                {
                    using(OdbcDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ML.CartaPorte.Art art = new ML.CartaPorte.Art();

                            art.CodDir = reader.GetString(0);
                            art.BieTra = reader.GetString(1);
                            art.DesCodfis = reader.GetString(2);
                            art.Pzas = reader.GetString(3);
                            art.PesArt = reader.GetDecimal(4);

                            arts.Add(art);  
                        }
                    }
                }
                decimal pesoBruto = arts.Sum(x => x.PesArt);

                List<ML.CartaPorte.IntTim24> intList = new List<ML.CartaPorte.IntTim24>();

                for (int i = 0; i < paradas.Count; i++)
                {
                    var paradaActual = paradas[i];

                    if (paradaActual.Des == null)
                        continue;

                    var paradaAnte = paradas[i - 1];

                    var artsPorCodDir = arts
                        .Where(a => a.CodDir == paradaActual.CodDir);

                    foreach (var art in artsPorCodDir)
                    {
                        intList.Add(new IntTim24
                        {
                            bie_tra = art.BieTra,
                            des_tra = art.DesCodfis,
                            id_ori = paradaAnte.Ori,
                            id_des = paradaActual.Des,
                            num_pza = decimal.Parse(art.Pzas),
                            pes_pza = art.PesArt,
                            id_uni = idUni,
                            cod_emp = 1m
                        });
                    }
                }

                result.Correct = true;
                result.Objects = new List<object>();
                result.Objects.Add(intList);
                result.Objects.Add(pesoBruto);
                
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error en armar articulos" + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static ML.Result BuildMedios(ML.CartaPorte.CartaInput cartaInput, string idUni, decimal pesoBruto)
        {
            ML.Result result = new ML.Result();
            try
            {
                ML.CartaPorte.TransTim24 transTim24 = new TransTim24();

                if (cartaInput.TraPro != null)
                {
                    transTim24 = new TransTim24
                    {
                        per_sct = cartaInput.TraPro.PerSct,
                        num_per = cartaInput.TraPro.NumPer,
                        con_veh = cartaInput.TraPro.ConVei,
                        pla_veh = cartaInput.TraPro.PlaVei,
                        mod_veh = cartaInput.TraPro.ModVei,
                        //tip_rem1 = " ",
                        //pla_rem1 = " ",
                        //tip_rem2 = " ",
                        //pla_rem2 = " ",
                        //aseg_med = " ",
                        //num_med = " ",
                        peso_bruto = pesoBruto,
                        //prima_seg = " ",
                        id_uni = idUni
                    };
                }

                ML.CartaPorte.OperTim2 opertim24 = new OperTim2
                {
                    rfc_ope = cartaInput.OperTda.RfcOpe,
                    num_lic = cartaInput.OperTda.LicOpe,
                    nom_ope = cartaInput.OperTda.NomOpe,
                    id_uni = idUni
                };

                result.Correct = true;
                result.Objects = new List<object>();
                result.Objects.Add(transTim24);
                result.Objects.Add(opertim24);
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error en armar transporte y operador" + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static ML.Result InsertInternalCartaPorte(ML.CartaPorte.CartaPorte cartaPorte, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    using (OdbcTransaction transaccion = connection.BeginTransaction())
                    {
                        try
                        {
                            string queryTra = $@"INSERT INTO trans_tim24 (per_sct, num_per, nom_ase, num_seg, con_veh, pla_veh, mod_veh, 
                                                                     tip_rem1, pla_rem1, tip_rem2, pla_rem2, aseg_carga, num_carga, 
                                                                     aseg_med, num_med, peso_bruto, id_uni, cod_emp)
                                         VALUES('{cartaPorte.transTim.per_sct}','{cartaPorte.transTim.num_per}','{cartaPorte.transTim.nom_ase}',
                                                 '{cartaPorte.transTim.num_seg}','{cartaPorte.transTim.con_veh}','{cartaPorte.transTim.pla_veh}','{cartaPorte.transTim.mod_veh}',
                                                 '{cartaPorte.transTim.tip_rem1}','{cartaPorte.transTim.pla_rem2}','{cartaPorte.transTim.tip_rem2}','{cartaPorte.transTim.pla_rem2}',
                                                 '{cartaPorte.transTim.aseg_carga}','{cartaPorte.transTim.num_carga}','{cartaPorte.transTim.aseg_med}',
                                                 '{cartaPorte.transTim.num_med}',{cartaPorte.transTim.peso_bruto},'{cartaPorte.transTim.id_uni}',
                                                 {cartaPorte.transTim.cod_emp})";

                            using (OdbcCommand cmdTra = new OdbcCommand(queryTra, connection, transaccion))
                            {
                                int rowsTra = cmdTra.ExecuteNonQuery();
                                if (rowsTra < 1)
                                {
                                    throw new Exception("No se pudieron ingresar los datos del transporte");
                                }
                            }
                            string queryOpe = $@"INSERT INTO oper_tim2 (rfc_ope, num_lic, nom_ope, tip_fig, part_trans, id_uni, cod_emp) 
                                         VALUES ('{cartaPorte.operTim.rfc_ope}', '{cartaPorte.operTim.num_lic}', '{cartaPorte.operTim.nom_ope}', '{cartaPorte.operTim.tip_fig}',
                                                 '{cartaPorte.operTim.part_trans}', '{cartaPorte.operTim.id_uni}', {cartaPorte.operTim.cod_emp})";

                            using (OdbcCommand cmdOper = new OdbcCommand(queryOpe, connection, transaccion))
                            {
                                int rowsOper = cmdOper.ExecuteNonQuery();
                                if (rowsOper < 1)
                                {
                                    throw new Exception("No se pudieron ingresar los datos del operador");
                                }
                            }

                            foreach (ML.CartaPorte.UbiTim2 ubi in cartaPorte.ubiTim)
                            {
                                string queryUbi = $@"INSERT INTO ubi_tim2 (rfc_rem, rfc_des, id_ori, id_des, fec_sal, fec_lle, dis_rec, id_uni, cod_emp) 
                                         VALUES ('{ubi.rfc_rem}', '{ubi.rfc_des}', '{ubi.id_ori}', '{ubi.id_des}', '{ubi.fec_sal}', '{ubi.fec_lle}',
                                                 {ubi.dis_rec}, '{ubi.id_uni}', {ubi.cod_emp})";
                                using (OdbcCommand cmdUbi = new OdbcCommand(queryUbi, connection, transaccion))
                                {
                                    int rowsUbi = cmdUbi.ExecuteNonQuery();
                                    if (rowsUbi < 1)
                                    {
                                        throw new Exception("No se pudo ingresar un dato de ubicacion");
                                    }
                                }
                            }

                            //dom
                            foreach (ML.CartaPorte.DomTim2 dom in cartaPorte.domTim)
                            {
                                string queryDom = $@"INSERT INTO dom_tim2 (des_ori, calle, num_ext, num_int, col, loca, ref, muni, est, pais, cod_pos, id_uni, cod_emp) 
                                             VALUES ('{dom.des_ori}', '{dom.calle}', '{dom.num_ext}', '{dom.num_int}', '{dom.col}', '{dom.loca}', '{dom.refe}',
                                                     '{dom.muni}', '{dom.est}', '{dom.pais}', '{dom.cod_pos}', '{dom.id_uni}', {dom.cod_emp})";
                                using (OdbcCommand cmdDom = new OdbcCommand(queryDom, connection, transaccion))
                                {
                                    int rowsDom = cmdDom.ExecuteNonQuery();
                                    if (rowsDom < 1)
                                    {
                                        throw new Exception("No se pudo ingresar un dato de domicilio");
                                    }
                                }
                            }

                            foreach (ML.CartaPorte.IntTim24 item in cartaPorte.intTim)
                            {
                                string queryItem = $@"INSERT INTO int_tim24 (bie_tra, des_tra, id_ori, id_des, num_pza, cla_uni, pes_pza, pedim, fra_aran, 
                                                                     mat_peli, cve_peli, emba, des_emba, tip_docum, doc_aduan, rf_clmpo, cofepris, 
                                                                     ingr_activo, quimico, deno_gene, deno_disti, fabrica, f_caduc, lote, farmac,  
                                                                     esp_transp, reg_sanita, permi_imp, vucem, cas, rs_emp_imp, san_plag_cofe, d_fabric, 
                                                                     d_formu, d_maquila, uso_autor, id_uni, cod_emp)
                                             VALUES ('{item.bie_tra}', '{item.des_tra}', '{item.id_ori}', '{item.id_des}', {item.num_pza}, '{item.cla_uni}', 
                                                     {item.pes_pza}, '{item.pedim}', '{item.fra_aran}', '{item.mat_peli}', '{item.cve_peli}', '{item.emba}', 
                                                     '{item.des_emba}', '{item.tip_docum}', '{item.doc_aduan}', '{item.rf_clmpo}', '{item.cofepris}', '{item.ingr_activo}', 
                                                     '{item.quimico}', '{item.deno_gene}', '{item.deno_disti}', '{item.fabrica}', '{item.f_caduc}', '{item.lote}', 
                                                     '{item.farmac}', '{item.esp_transp}', '{item.reg_sanita}', '{item.permi_imp}', '{item.vucem}', '{item.cas}', 
                                                     '{item.rs_emp_imp}', '{item.san_plag_cofe}', '{item.d_fabric}', '{item.d_formu}', '{item.d_maquila}', 
                                                     '{item.uso_autor}', '{item.id_uni}', {item.cod_emp})";
                                using (OdbcCommand cmdItem = new OdbcCommand(queryItem, connection, transaccion))
                                {
                                    int rowsItem = cmdItem.ExecuteNonQuery();
                                    if (rowsItem < 1)
                                    {
                                        throw new Exception("No se pudo ingresar un dato de un producto");
                                    }
                                }
                            }

                            //Insertar seguimientoooo

                            transaccion.Commit();
                            result.Correct = true;
                            result.Message = "Documento Interno ingresado correctamente ";
                        }
                        catch (Exception ex)
                        {
                            transaccion.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error al insertar" + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        private static ML.Result InsertTracking(ML.CartaPorte.CartaInput cartaInput, string idUni, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    string query = $@"INSERT INTO cartaporte_tracking (pto_alm,car_sal,id_uni,doc_typ,fec_gen,fec_sal,rfc_ope,num_eco)
                                        VALUES(?,?,?,?,?,?,?,?);";

                    string eco = cartaInput.TraPro == null ? "X" : cartaInput.TraPro.NumEco;

                    using (OdbcCommand cmd = new OdbcCommand(query,connection))
                    {
                        cmd.Parameters.AddWithValue("pto_alm", cartaInput.PtoAlm);
                        cmd.Parameters.AddWithValue("car_sal", cartaInput.CarSal);
                        cmd.Parameters.AddWithValue("id_uni", idUni);
                        cmd.Parameters.AddWithValue("doc_typ", cartaInput.Tipo);
                        cmd.Parameters.AddWithValue("fec_gen", DateTime.Now);
                        cmd.Parameters.AddWithValue("fec_sal", DateTime.ParseExact(cartaInput.FecSal, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("rfc_ope", cartaInput.OperTda.RfcOpe);
                        cmd.Parameters.AddWithValue("num_eco", eco);

                        int rowsAffected = cmd.ExecuteNonQuery();
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error al insertar tracking" + ex.Message;
                result.Ex = ex;
            }
            return result;
        }

        private static ML.Result SendInfo(ML.CartaPorte.CartaPorte cartaPorte, ML.CartaPorte.CartaInput cartaInput, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                List<Mercancia> mercancias = cartaPorte.intTim
                                .Select(x => new Mercancia
                                {
                                    BieTra = x.bie_tra,
                                    DesTra = x.des_tra,
                                    IdOri = x.id_ori,
                                    IdDes = x.id_des,
                                    NumPza = x.num_pza?.ToString(),
                                    ClaUni = x.cla_uni,
                                    PesPza = x.pes_pza?.ToString(),
                                    IdUni = x.id_uni,
                                    CodEmp = x.cod_emp?.ToString()
                                })
                                .ToList();

                StringBuilder mer = new StringBuilder();

                PropertyInfo[] propMer = typeof(Mercancia).GetProperties();

                mer.AppendLine(string.Join(",", propMer.Select(p => p.Name)));

                foreach (var m in mercancias)
                {
                    var valores = propMer.Select(p =>
                    {
                        var value = p.GetValue(m)?.ToString() ?? "";
                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = "\"" + value.Replace("\"", "\"\"") + "\"";
                        }
                        return value;
                    });

                    mer.AppendLine(string.Join(",", valores));
                }

                StringBuilder dom = new StringBuilder();

                var propDom = typeof(DomTim2).GetProperties();

                dom.AppendLine(string.Join(",", propDom.Select(p => p.Name)));

                foreach (var d in cartaPorte.domTim)
                {
                    var valores = propDom.Select(p =>
                    {
                        var value = p.GetValue(d)?.ToString() ?? "";
                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = "\"" + value.Replace("\"", "\"\"") + "\"";
                        }
                        return value;
                    });

                    dom.AppendLine(string.Join(",", valores));
                }

                SendEmail(cartaInput,mer.ToString(),dom.ToString());

                result.Correct = true;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error al enviar los datos de cartaporte" + ex.Message;
                result.Ex = ex;
            }
            return result;
        }
        public static void SendEmail(ML.CartaPorte.CartaInput cartaInput, string csvMercancias, string csvDomicilios)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("10.128.14.17")
                {
                    UseDefaultCredentials = true,
                };

                MailMessage message = new MailMessage();
                message.From = new MailAddress($@"cartaporte@sears.com.mx");
                message.Subject = $"Carta Porte Proveedor - Carga {cartaInput.CarSal} - Operador {cartaInput.OperTda.NomOpe}";

                string correos = "aaoran@sears.com.mx";
                string[] correosList = correos.Split(" ");
                foreach (string correo in correosList)
                {
                    message.To.Add(correo);
                }

                string archMer = $"Mercancias_{cartaInput.CarSal}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string archDom = $"Domicilios_{cartaInput.CarSal}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                var cuerpoMensaje = $@"
                        <html>
                        <head>
                            <title>Carta Porte Proveedor</title>
                        </head>
                        <body style='font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f0f0f0;'>
                            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden;'>
                                <div style='background-color: #1e3a8a; height: 20px;'></div>
                                <div style='padding: 20px;'>
                                    <h3 style='color: #333333;'>Carta Porte Proveedor - carga {cartaInput.CarSal}</h3>
                                    <p style='color: #666666;'>Se adjuntan los archivos para la entrega del folio <strong>{cartaInput.CarSal}</strong> y operador <strong>{cartaInput.OperTda.NomOpe}</strong>.</p>
                                    <ul style='color: #666666;'>
                                        <li>Datos de mercancía {archMer}(CSV adjunto)</li>
                                        <li>Datos de domicilios {archDom}(CSV adjunto)</li>
                                    </ul>
                                    <p style='color: #666666;'>Fecha de envío: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                                    <br>
                                    <p style='color: #666666;'>Atentamente,</p>
                                    <p style='color: #666666;'>Sears</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                message.IsBodyHtml = true;
                message.Body = cuerpoMensaje;

                byte[] merBytes = Encoding.UTF8.GetBytes(csvMercancias);
                Attachment mercanciasAdjunto = new Attachment(
                    new MemoryStream(merBytes),
                    archMer,
                    "text/csv"
                );
                message.Attachments.Add(mercanciasAdjunto);

                byte[] domBytes = Encoding.UTF8.GetBytes(csvDomicilios);
                Attachment domiciliosAdjunto = new Attachment(
                    new MemoryStream(domBytes),
                    archDom,
                    "text/csv"
                );
                message.Attachments.Add(domiciliosAdjunto);

                smtpClient.Send(message);

            }
            catch (Exception ex)
            {
                //throw;
            }
        }
    }
}
