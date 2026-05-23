using BL.Menu;
using ML.User;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.User
{
    public class User
    {
        public static List<MenuItem> GetMenu(string usu_id,string sub_rol, string mode)
        {
            List<MenuItem> menuG = new List<MenuItem>();

            if (usu_id != null)
            {
                menuG.Add(new MenuItem("Home", "Menu de inicio Sistema Hermes","Home", "Index", "bi-house"));
            }
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = sub_rol != "OPE" ?
                            $@"SELECT TRIM(D.title),TRIM(D.descr),TRIM(D.contr),TRIM(D.action),TRIM(D.icons)
                                        FROM ora_lga_usu A, hermes_rol B, hermes_rol_screen C, hermes_screen D
                                        WHERE A.usu_id = {usu_id}
                                        AND A.sub_rol = '{sub_rol}'
                                        AND B.sub_rol = A.sub_rol
                                        AND C.sub_rol = B.sub_rol
                                        AND D.id_pant = C.id_pant"
                            :
                            $@"SELECT TRIM(D.title),TRIM(D.descr),TRIM(D.contr),TRIM(D.action),TRIM(D.icons)
                                        FROM hermes_rol_screen C, hermes_screen D
                                        WHERE C.sub_rol = '{sub_rol}'
                                        AND D.id_pant = C.id_pant";

                            //$@"SELECT TRIM(D.title),TRIM(D.descr),TRIM(D.contr),TRIM(D.action),TRIM(D.icons)
                            //            FROM ora_operadores A, hermes_rol B, hermes_rol_screen C, hermes_screen D
                            //            WHERE A.rfc_ope = '{usu_id}'
                            //            AND B.sub_rol = '{sub_rol}'
                            //            AND C.sub_rol = B.sub_rol
                            //            AND D.id_pant = C.id_pant";


                    using (OdbcCommand cmd = new OdbcCommand(query, connection))
                    {
                        List<MenuItem> menu = new List<MenuItem>();

                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MenuItem men = new(
                                                reader.GetString(0),
                                                reader.GetString(1),
                                                reader.GetString(2),
                                                reader.GetString(3),
                                                reader.GetString(4)
                                            );
                                menu.Add(men);
                            }
                        }

                        menuG.AddRange(menu);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return menuG;
        }

        public static ML.Result GetLgaUsu(string usu_id, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using(OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"SELECT usu_id,TRIM(usu_nombre)
                                        FROM dblga@lga_prod:lgausuario
                                        WHERE usu_id = ?
                                        AND usu_status = 1
                                        AND usu_id NOT IN (SELECT usu_id FROM ora_lga_usu)";

                    ML.User.User user = new ML.User.User();

                    using(OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("usu_id", usu_id);
                        using(OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user.UsuId = reader.GetString(0);
                                user.Name = reader.GetString(1);
                            }
                            else
                            {
                                throw new Exception($@"No se encontro el usuario {usu_id}, esta inactivo en logistica o ya existente");
                            }
                        }
                    }

                    result.Correct = true;
                    result.Object = user;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al obtener usuario logistica {ex.Message}";
            }
            return result;
        }

        public static ML.Result GetAlma(string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using(OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"SELECT cen_pto
                                        FROM ora_fac_go
                                        WHERE is_fac = 'T'
                                        ";

                    List<string> almaList = new List<string>();

                    using(OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        using(OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                almaList.Add(reader.GetString(0));
                            }
                        }
                    }

                    result.Correct = true;
                    result.Object = almaList;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al obtener almacenes {ex.Message}";
            }
            return result;
        }

        public static ML.Result GetAllUsers(string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"SELECT A.usu_id,TRIM(B.usu_nombre),TRIM(A.sub_rol),TRIM(A.almacenes)
                                            FROM ora_lga_usu A, dblga@lga_prod:lgausuario B
                                            WHERE B.usu_id = A.usu_id
                                            AND B.usu_status = 1";

                    List<ML.User.User> userList = new List<ML.User.User>();

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ML.User.User user = new ML.User.User();

                                user.UsuId = reader.GetString(0);
                                user.Name = reader.GetString(1);
                                user.SubRol = reader.GetString(2);
                                user.Almac = reader.GetString(3);

                                userList.Add(user);
                            }
                        }
                    }

                    result.Correct = true;
                    result.Object = userList;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al obtener usuarios {ex.Message}";
            }
            return result;
        }

        public static ML.Result AddUser(ML.User.User user, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using(OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"INSERT INTO ora_lga_usu(usu_id,sub_rol,almacenes) VALUES(?,?,?)";

                    using(OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("usu_id", user.UsuId);
                        command.Parameters.AddWithValue("sub_rol",user.SubRol);
                        command.Parameters.AddWithValue("almacenes",user.Almac);

                        int rowsAffected = command.ExecuteNonQuery();

                        if(rowsAffected < 1)
                        {
                            throw new Exception($@"Error no se inserto {user.UsuId}");
                        }
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al insertar usuario {ex.Message}";
            }
            return result;
        }
        public static ML.Result GetRoles(string usu_rol,string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string exclude = usu_rol == "ADM" ? 
                                            "'X'" : 
                                            $@"'ADM','GGB'";

                    string query = $@"SELECT TRIM(sub_rol),TRIM(des_rol)
                                        FROM hermes_rol
                                        WHERE sub_rol NOT IN ({exclude})";

                    List<ML.User.Rol> rolList = new List<ML.User.Rol>();

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ML.User.Rol rol = new ML.User.Rol();

                                rol.SubRol = reader.GetString(0);
                                rol.DesRol = reader.GetString(1);

                                rolList.Add(rol);
                            }
                        }
                    }

                    result.Correct = true;
                    result.Object = rolList;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al obtener Roles {ex.Message}";
            }
            return result;
        }
        public static ML.Result UpdateUserRol(ML.User.User user, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();
                        
                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"UPDATE ora_lga_usu SET sub_rol = '{user.SubRol}' WHERE usu_id = {user.UsuId}";

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        //command.Parameters.AddWithValue("usu_id", user.UsuId);
                        //command.Parameters.AddWithValue("sub_rol", user.SubRol);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            throw new Exception($@"Error no se actualizo {user.UsuId}");
                        }
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al actualizar usuario {ex.Message}";
            }
            return result;
        }

        public static ML.Result AddRol(ML.User.Rol rol, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"INSERT INTO hermes_rol(sub_rol,des_rol) VALUES(?,?);";

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", rol.SubRol);
                        command.Parameters.AddWithValue("des_rol", rol.DesRol);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            throw new Exception($@"Error no se inserto {rol.SubRol}");
                        }
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al insertar rol {ex.Message}";
            }
            return result;
        }
        public static ML.Result DelRol(ML.User.Rol rol, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string queryValidate = $@"SELECT COUNT(*)
                                            FROM ora_lga_usu
                                            WHERE sub_rol = ?";

                    bool exist = false;
                    using (OdbcCommand command = new OdbcCommand(queryValidate, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", rol.SubRol);

                        using(OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                exist = reader.GetInt16(0) > 0 ? true : false;
                            }
                        }
                    }

                    if(exist)
                    {
                        throw new Exception($@"Hay usuarios con este rol activo, no se puede eliminar");
                    }

                    string queryHas = $@"SELECT COUNT(*)
                                            FROM hermes_rol_screen
                                            WHERE sub_rol = ?";

                    bool hasScreen = false;
                    using (OdbcCommand command = new OdbcCommand(queryHas, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", rol.SubRol);
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hasScreen = reader.GetInt16(0) > 0 ? true : false;
                            }
                        }
                    }

                    if (hasScreen)
                    {
                        throw new Exception($@"Hay pantallas con este rol activo, no se puede eliminar");
                    }

                    string query = $@"DELETE FROM hermes_rol WHERE sub_rol = ?";

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", rol.SubRol);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            throw new Exception($@"Error no se elimino {rol.SubRol}");
                        }
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al eliminar rol {ex.Message}";
            }
            return result;
        }

        public static ML.Result GetScreens(string sub_rol, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = @"SELECT 
                                S.id_pant,
                                S.title,
                                CASE 
                                    WHEN R.sub_rol IS NOT NULL THEN 1 
                                    ELSE 0 
                                END AS assigned
                             FROM hermes_screen S
                             LEFT JOIN hermes_rol_screen R 
                                ON R.id_pant = S.id_pant 
                                AND R.sub_rol = ?";

                    List<ML.User.Screen> screenList = new List<ML.User.Screen>();

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", sub_rol);

                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ML.User.Screen screen = new ML.User.Screen
                                {
                                    IdPant = reader.GetString(0),
                                    Title = reader.GetString(1),
                                    Assigned = reader.GetInt16(2) == 1
                                };

                                screenList.Add(screen);
                            }
                        }
                    }

                    result.Correct = true;
                    result.Object = screenList;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al obtener Pantallas {ex.Message}";
            }
            return result;
        }

        public static ML.Result AddScreenToRol(string sub_rol, string id_pant, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"INSERT INTO hermes_rol_screen(sub_rol,id_pant) VALUES (?,?);";

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", sub_rol);
                        command.Parameters.AddWithValue("id_pant", id_pant);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            throw new Exception($@"Error no se inserto {sub_rol} y {id_pant}");
                        }
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al insertar rol y pantalla {ex.Message}";
            }
            return result;
        }
        public static ML.Result DelScreenToRol(string sub_rol, string id_pant, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"DELETE FROM hermes_rol_screen
                                                WHERE sub_rol = ?
                                                AND id_pant = ?";

                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("sub_rol", sub_rol);
                        command.Parameters.AddWithValue("id_pant", id_pant);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            throw new Exception($@"Error no se elimino {sub_rol} y {id_pant}");
                        }
                    }

                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Ex = ex;
                result.Correct = false;
                result.Message = $@"Error al eliminar rol y pantalla {ex.Message}";
            }
            return result;
        }
    }
}
