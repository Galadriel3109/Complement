using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Menu
{
    public class MenuService
    {
        public static List<MenuItem> GetMenu(string usu_id, string cv_area, string sub_rol)
        {
            var menu = new List<MenuItem>();

            if (usu_id != null)
            {
                menu.Add(new MenuItem("Home", "", "Home", "Index", "bi-house"));
            }

            if (sub_rol == "SIS")
            {
                menu.AddRange(GetAllOptions());
                return menu;
            }

            if (cv_area == "CIC" && sub_rol == "CIC")
            {
                menu.Add(new MenuItem("Ordenes", "", "Importations", "GetOrders", "bi-receipt"));
                menu.Add(new MenuItem("Consulta Hija", "", "Importations", "GetRelation", "bi-search bi-receipt"));
                menu.Add(new MenuItem("MatchGTM-LGA", "", "Importations", "MatchOrders", "bi-arrow-left-right"));
            }

            if (cv_area == "CON" || cv_area == "GGB" || sub_rol == "CIN"|| sub_rol == "SAC")
            {
                menu.Add(new MenuItem("Rutas", "", "TrackingManager", "GetTrackingPerDay", "bi-truck bi-truck"));
                menu.Add(new MenuItem("Regresos", "", "TrackingManager", "ReturnedOrders", "bi-box-seam bi-arrow-return-left"));
                menu.Add(new MenuItem("Busqueda", "", "BaseControl", "GetOrdersPerData", "bi-search"));
                //menu.Add(new MenuItem("MatcxDia", "LastMileDelivery", "GetShipmentsByDay", "bi-calendar3-event"));
                //menu.Add(new MenuItem("MatchxRango", "LastMileDelivery", "GetShipmentsByQuery", "bi-calendar2-week"));
            }

            if (sub_rol == "BAS" || sub_rol == "INT" || sub_rol == "SCS" || cv_area == "GGB")
            {
                menu.Add(new MenuItem("BaseControl", "", "BaseControl", "BaseControl", "bi-clipboard-check"));
            }

            if (sub_rol == "BAS" ||cv_area == "GGB")
            {
                menu.Add(new MenuItem("BaseOperador", "", "BaseControl", "BaseOperatorGetOpenRoutes", "bi-clipboard-check bi-people"));
                //new("BaseOperador", "BaseControl", "BaseOperatorGetOpenRoutes", "bi-clipboard-check bi-people"),
            }

            if (sub_rol == "ULM" ||cv_area == "GGB")
            {
                menu.Add(new MenuItem("Operadores", "", "OutbondShipmentOperator", "Operadores", "bi-people bi-truck"));
                menu.Add(new MenuItem("Carga de salida", "", "OutbondShipmentOperator", "OutbondShipment", "bi-box-seam"));


                //new("BaseOperador", "BaseControl", "BaseOperatorGetOpenRoutes", "bi-clipboard-check bi-people"),
            }

            //if (sub_rol == "SCS" || cv_area == "GGB")
            //{
            //    menu.Add(new MenuItem("BaseControlOTM", "BaseControl", "BaseControlPast", "bi-clipboard-check-fill"));
            //}

            if (cv_area == "OPE")
            {
                //new("Asignado", "Operator", "GetAssignedRoute", "bi-signpost-2 bi-person-check"),
                //new("Historico", "Operator", "GetHistorical", "bi-signpost-2 bi-archive")
                menu.Add(new MenuItem("Asignado", "","Operator", "GetAssignedRoute", "bi-signpost-2 bi-person-check"));
                menu.Add(new MenuItem("Historico", "", "Operator", "GetHistorical", "bi-signpost-2 bi-archive"));
            }

            return menu;
        }
        public static List<MenuItem> GetMenu(string usu_id, string cv_area, string sub_rol, string mode)
        {
            List<MenuItem> menuG = new List<MenuItem>();

            if (usu_id != null)
            {
                menuG.Add(new MenuItem("Home", "","Home", "Index", "bi-house"));
            }
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    OdbcCommand cmdIsolation = new OdbcCommand("SET ISOLATION TO DIRTY READ;", connection);
                    cmdIsolation.ExecuteNonQuery();

                    string query = $@"SELECT TRIM(D.title),TRIM(D.descr)TRIM(D.contr),TRIM(D.action),TRIM(D.icons)
                                        FROM ora_lga_usu A, hermes_rol B, hermes_rol_screen C, hermes_rol_screen D
                                        WHERE A.usu_id = ?
                                        AND A.sub_rol = ?
                                        AND B.sub_rol = A.sub_rol
                                        AND C.sub_rol = B.sub_rol
                                        AND D.id_pant = C.id_pant";

                    using(OdbcCommand cmd = new OdbcCommand(query,connection))
                    {
                        cmd.Parameters.AddWithValue("usu_id",usu_id);
                        cmd.Parameters.AddWithValue("sub_rol",sub_rol);

                        List<MenuItem> menu = new List<MenuItem>();

                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
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

        private static List<MenuItem> GetAllOptions()
        {
            return new List<MenuItem>
            {
                new("Ordenes", "","Importations", "GetOrders", "bi-receipt"),
                new("Consulta Hija", "","Importations", "GetRelation", "bi-search bi-receipt"),
                new("MatchGTM-LGA", "","Importations", "MatchOrders", "bi-arrow-left-right"),
                new("Rutas", "","TrackingManager", "GetTrackingPerDay", "bi-truck bi-truck"),
                new("Regresos", "","TrackingManager", "ReturnedOrders", "bi-box-seam bi-arrow-return-left"),
                //new("MatcxDia", "","LastMileDelivery", "GetShipmentsByDay", "bi-calendar3-event"),
                //new("MatchxRango", "","LastMileDelivery", "GetShipmentsByQuery", "bi-calendar2-week"),
                new("Busqueda", "","BaseControl", "GetOrdersPerData", "bi-search"),
                new("BaseControl", "","BaseControl", "BaseControl", "bi-clipboard-check"),
                //new("BaseControlOTM", "","BaseControl", "BaseControlPast", "bi-clipboard-check-fill"),
                new("BaseOperador", "","BaseControl", "BaseOperatorGetOpenRoutes", "bi-clipboard-check bi-people"),
                new("BaseMasivo", "","BaseControl", "BaseMasivo", "bi-clipboard-check bi-collection-fill"),
                new("Mantenimiento", "","Maintenance", "InfoByScn", "bi-geo-alt"),
                new("Pend. Confirmacion", "","Maintenance", "GetToConfirm", "bi-calendar-check"),
                new("Carta Porte", "","CartaPorte", "Index", "bi-file-earmark-text bi-truck"),
                new("Ordenes Listas","", "DeliveryPlanner", "GetReadyOrdersPerDate", "bi-file-earmark-check bi-send"),
                new("Planeacion","", "DeliveryPlanner", "Planning", "bi-map bi-diagram-3"),
                new("Planeaciones", "","DeliveryPlanner", "Plans", "bi-map bi-truck"),
                new("Operadores", "","OutbondShipmentOperator", "Operadores", "bi-people bi-truck"),
                new("Carga de salida", "","OutbondShipmentOperator", "OutbondShipment", "bi-box-seam"),
                new("ReSender", "","ReSender", "ReSender", "bi-arrow-clockwise bi-send"),
                new("Asignado", "","Operator", "GetAssignedRoute", "bi-signpost-2 bi-person-check"),
                new("Historico","", "Operator", "GetHistorical", "bi-signpost-2 bi-archive"),
                new("Usuario", "","User", "User", "bi-people")

            };
        }
    }

    public record MenuItem(string Title, string Description, string Controller, string Action, string Icon);    
}
