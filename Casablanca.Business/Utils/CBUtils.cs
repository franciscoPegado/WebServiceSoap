using Benner.Tecnologia.Business;
using Benner.Tecnologia.Common;
using Benner.Tecnologia.Common.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Casablanca.Business.Utils
{
    public class CBUtils
    {
        const string LinkDownloadPortalCorporativo = @"http://erp.casablancaturismo.com.br/PortalCorporativo/DownloadFile.ashx";

        public static bool EhUsuarioCliente()
        {
            try
            {
                Query q = new Query("SELECT A.HANDLE FROM BB_USUARIOPESSOAS A WHERE A.USUARIO = :PUSUARIO AND A.PESSOA IS NOT NULL AND ((A.EHCLIENTE = 'S') OR (A.GRUPOEMPRESARIAL = 'S')) ");
                q.Parameters.Add(new Parameter("PUSUARIO", DataType.Integer, BennerContext.Security.GetLoggedUserHandle()));

                Entities<EntityBase> usuarios = q.Execute();

                return usuarios.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static string MontarLinkDownload(string pTabela, string pCampo, long pHandle)
        {
            LinkDefinition link = new LinkDefinition();
            link.Url = LinkDownloadPortalCorporativo;
            link.Parameters["$SystemInstanceName$"] = "BACKOFFICE_XE3";
            link.Parameters["$EntityDefinitionName$"] = pTabela;
            link.Parameters["$FieldName$"] = pCampo;
            link.Parameters["$EntityHandle$"] = new Handle(pHandle);
            return link.GetEncodedUrl();
        }

        public static object FromXml(string Xml, System.Type ObjType)
        {
            XmlSerializer ser = new XmlSerializer(ObjType);
            StringReader stringReader = new StringReader(Xml);
            XmlTextReader xmlReader = new XmlTextReader(stringReader);
            object obj = ser.Deserialize(xmlReader);
            xmlReader.Close();
            stringReader.Close();
            return obj;
        }

        public static string ToXml(Object prObjeto)
        {
            string _result = string.Empty;
            MemoryStream _ms = new MemoryStream();
            StreamWriter _sw = new StreamWriter(_ms);

            XmlWriterSettings _settings = new XmlWriterSettings();
            _settings.Encoding = Encoding.UTF8;
            _settings.Indent = true;

            XmlWriter _writer = XmlWriter.Create(_sw, _settings);
            XmlSerializer serializer = new XmlSerializer(prObjeto.GetType());
            serializer.Serialize(_writer, prObjeto);
            _writer.Flush();
            _writer.Close();

            StreamReader _sr = new StreamReader(_ms);
            _ms.Position = 0;
            _result = _sr.ReadToEnd();
            _sr.Close();

            return _result;
        }
    }
}
