using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMapper.UnitTests
{
    public class ClienteEntity
    {
        public int CodigoCliente { get; set; }
        public string Nome { get; set; }
        public EnderecoEntity Endereco { get; set; }
        public StatusCliente? Status { get; set; }
        public DateTime? DataCadastro { get; set; }
        public List<TelefoneEntity> Telefones { get; set; }
    }

    public class ClienteConsultaDto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public EnderecoDto Endereco { get; set; }
        public string Cidade { get; set; }
        public string Status { get; set; }
        public string DataCadastro { get; set; }
        public List<TelefoneDto> Telefones { get; set; }
    }

    public class EnderecoEntity
    {
        public string Cidade { get; set; }
    }

    public class EnderecoDto
    {
        public string Cidade { get; set; }
    }

    public class TelefoneEntity
    {
        public string Numero { get; set; }
    }

    public class TelefoneDto
    {
        public string Numero { get; set; }
    }

    public enum StatusCliente
    {
        Ativo,
        Inativo
    }

}
