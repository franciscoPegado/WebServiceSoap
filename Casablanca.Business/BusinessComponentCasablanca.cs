using Benner.Tecnologia.Business;
using Casablanca.Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.Business
{
    public class BusinessComponentCasablanca<T> : BusinessComponent<T> where T : new ()
    {
        public BusinessComponentCasablanca()
        {
            if (!CBUtils.EhUsuarioCliente())
                throw new Exception("Acesso negado! Usuário não está ligado a nenhum cliente!");
        }
    }
}
