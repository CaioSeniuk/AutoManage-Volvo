using Microsoft.EntityFrameworkCore;
using AutoManage.Controllers;
using AutoManage.Data;
using AutoManage.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace AutoManage.Tests
{
    public class VeiculosControllerTests
    {
        private AutoManageContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AutoManageContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Novo banco para cada teste
                .Options;

            var context = new AutoManageContext(options);
            return context;
        }

        [Fact]
        public async Task PostVeiculo_DeveRetornarCreated_QuandoVeiculoValido()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new VeiculosController(context);

            var veiculo = new Veiculo
            {
                Chassi = "CHASSI_TESTE_001",
                Modelo = "FH16",
                Ano = 2025,
                Valor = 500000m
            };

            // Act
            var result = await controller.PostVeiculo(veiculo);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var veiculoCriado = Assert.IsType<Veiculo>(createdResult.Value);
            Assert.Equal("CHASSI_TESTE_001", veiculoCriado.Chassi);
        }

        [Fact]
        public async Task PostVeiculo_DeveRetornarBadRequest_QuandoChassiDuplicado()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            // Popula o banco com um veículo existente
            context.Veiculos.Add(new Veiculo 
            { 
                Chassi = "CHASSI_DUPLICADO", 
                Modelo = "FM", 
                Ano = 2024,
                Valor = 400000m
            });
            await context.SaveChangesAsync();

            var controller = new VeiculosController(context);

            var veiculoNovo = new Veiculo
            {
                Chassi = "CHASSI_DUPLICADO", // Tentando usar o mesmo chassi
                Modelo = "FMX",
                Ano = 2025,
                Valor = 450000m
            };

            // Act
            var result = await controller.PostVeiculo(veiculoNovo);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            // Verifica se a mensagem de erro veio da nossa Chain of Responsibility
            var error = result.Result as BadRequestObjectResult;
            Assert.Contains("Chassi já cadastrado", error.Value.ToString());
        }

        [Fact]
        public async Task PostVeiculo_DeveRetornarBadRequest_QuandoProprietarioNaoExiste()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new VeiculosController(context);

            var veiculo = new Veiculo
            {
                Chassi = "CHASSI_TESTE_002",
                Modelo = "VM",
                Ano = 2025,
                Valor = 300000m,
                ProprietarioId = 999 // ID que não existe no banco
            };

            // Act
            var result = await controller.PostVeiculo(veiculo);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var error = result.Result as BadRequestObjectResult;
            Assert.Contains("Proprietário informado não foi encontrado", error.Value.ToString());
        }

        [Fact]
        public async Task PostVeiculo_DeveRetornarCreated_QuandoProprietarioExiste()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            // Cria um proprietário válido
            var proprietario = new Proprietario 
            { 
                Nome = "Transportadora Teste", 
                CPF_CNPJ = "12345678000199" 
            };
            context.Proprietarios.Add(proprietario);
            await context.SaveChangesAsync();

            var controller = new VeiculosController(context);

            var veiculo = new Veiculo
            {
                Chassi = "CHASSI_TESTE_003",
                Modelo = "FH",
                Ano = 2025,
                Valor = 600000m,
                ProprietarioId = proprietario.Id // Vincula ao ID criado
            };

            // Act
            var result = await controller.PostVeiculo(veiculo);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
        }
    }
}
