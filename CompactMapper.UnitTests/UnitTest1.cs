using Xunit;

namespace CompactMapper.UnitTests
{
    public class CompactMapperTests
    {
        public CompactMapperTests()
        {
            // Register custom mapping
            CompactMapperExtension.AddCustomMapping<ClienteEntity, ClienteConsultaDto>((src, dest) =>
            {
                dest.Codigo = src.CodigoCliente.ToString();
                dest.Cidade = src.Endereco?.Cidade;
            });
        }

        [Fact]
        public void Should_Map_Simple_Properties()
        {
            var entity = new ClienteEntity { CodigoCliente = 1, Nome = "João" };
            var dto = entity.MapTo<ClienteConsultaDto>();

            Assert.Equal("1", dto.Codigo);
            Assert.Equal("João", dto.Nome);
        }
        [Fact]
        public void Should_Map_Nested_Properties()
        {
            var entity = new ClienteEntity
            {
                Endereco = new EnderecoEntity { Cidade = "São Paulo" }
            };

            var dto = entity.MapTo<ClienteConsultaDto>();

            Assert.NotNull(dto.Cidade);
            Assert.Equal("São Paulo", dto.Cidade);
        }

        [Fact]
        public void Should_Map_Nested_Objects()
        {
            var entity = new ClienteEntity
            {
                Endereco = new EnderecoEntity { Cidade = "São Paulo" }
            };

            var dto = entity.MapTo<ClienteConsultaDto>();

            Assert.NotNull(dto.Endereco);
            Assert.Equal("São Paulo", dto.Endereco.Cidade);
        }

        [Fact]
        public void Should_Map_Enum_To_String()
        {
            var entity = new ClienteEntity { Status = StatusCliente.Ativo };
            var dto = entity.MapTo<ClienteConsultaDto>();

            Assert.Equal("Ativo", dto.Status);
        }

        [Fact]
        public void Should_Map_DateTime_To_String()
        {
            var date = new DateTime(2024, 1, 1);
            var entity = new ClienteEntity { DataCadastro = date };

            var dto = entity.MapTo<ClienteConsultaDto>();
            Assert.Equal(date.ToString(), dto.DataCadastro);
        }

        [Fact]
        public void Should_Map_Collections()
        {
            var entity = new ClienteEntity
            {
                Telefones = new List<TelefoneEntity>
            {
                new TelefoneEntity { Numero = "123" },
                new TelefoneEntity { Numero = "456" }
            }
            };

            var dto = entity.MapTo<ClienteConsultaDto>();

            Assert.NotNull(dto.Telefones);
            Assert.Equal(2, dto.Telefones.Count);
            Assert.Equal("123", dto.Telefones[0].Numero);
            Assert.Equal("456", dto.Telefones[1].Numero);
        }

        [Fact]
        public void Should_Apply_Custom_Value_Transformer()
        {
            var entity = new ClienteEntity { Nome = "Maria" };
            var dto = entity.MapTo<ClienteConsultaDto>(valueTransformer: (prop, val) =>
            {
                return prop == "Nome" ? ((string)val).ToUpper() : val;
            });

            Assert.Equal("MARIA", dto.Nome);
        }

        [Fact]
        public void Should_Ignore_Null_Values()
        {
            var entity = new ClienteEntity { Nome = null };
            var dto = entity.MapTo<ClienteConsultaDto>();

            Assert.Null(dto.Nome);
        }
    }

}