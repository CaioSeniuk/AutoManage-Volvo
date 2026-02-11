using Microsoft.EntityFrameworkCore;
using AutoManage.Models;

namespace AutoManage.Data
{
    /// <summary>
    /// contexto do banco de dados para o sistema automanage
    /// </summary>
    public class AutoManageContext : DbContext
    {
        public AutoManageContext(DbContextOptions<AutoManageContext> options)
            : base(options)
        {
        }


        // dbsets representam as tabelas no banco de dados
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Proprietario> Proprietarios { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        
        // sistema de pecas genuinas volvo
        public DbSet<Peca> Pecas { get; set; }
        public DbSet<PedidoPeca> PedidosPecas { get; set; }
        public DbSet<ItemPedidoPeca> ItensPedidoPeca { get; set; }
        
        // autenticacao
        public DbSet<Usuario> Usuarios { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configuracao de usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // configuracao de relacionamentos e indices

            // veiculo -> proprietario (muitos-para-um)
            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.Proprietario)
                .WithMany(p => p.Veiculos)
                .HasForeignKey(v => v.ProprietarioId)
                .OnDelete(DeleteBehavior.SetNull); // se deletar proprietario, veiculo fica sem dono

            // venda -> veiculo (muitos-para-um)
            modelBuilder.Entity<Venda>()
                .HasOne(v => v.Veiculo)
                .WithMany(ve => ve.Vendas)
                .HasForeignKey(v => v.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict); // nao permite deletar veiculo se tiver vendas

            // venda -> vendedor (muitos-para-um)
            modelBuilder.Entity<Venda>()
                .HasOne(v => v.Vendedor)
                .WithMany(ve => ve.Vendas)
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.Restrict); // nao permite deletar vendedor se tiver vendas

            // indices para melhorar performance de consultas
            modelBuilder.Entity<Veiculo>()
                .HasIndex(v => v.VersaoMotor);

            modelBuilder.Entity<Veiculo>()
                .HasIndex(v => v.Quilometragem);

            modelBuilder.Entity<Proprietario>()
                .HasIndex(p => p.CPF_CNPJ)
                .IsUnique();

            modelBuilder.Entity<Venda>()
                .HasIndex(v => v.DataVenda);

            // configuracoes do sistema de pecas
            modelBuilder.Entity<PedidoPeca>()
                .HasOne(pp => pp.Vendedor)
                .WithMany()
                .HasForeignKey(pp => pp.VendedorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ItemPedidoPeca>()
                .HasOne(i => i.PedidoPeca)
                .WithMany(pp => pp.Itens)
                .HasForeignKey(i => i.PedidoPecaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemPedidoPeca>()
                .HasOne(i => i.Peca)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(i => i.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            // indices para pecas
            modelBuilder.Entity<Peca>()
                .HasIndex(p => p.CodigoPeca)
                .IsUnique();

            modelBuilder.Entity<Peca>()
                .HasIndex(p => p.EstoqueAtual);
        }
    }
}
