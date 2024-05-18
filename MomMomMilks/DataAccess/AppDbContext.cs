﻿using BusinessObject.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace DataAccess
{
    public class AppDbContext : IdentityDbContext<AppUser,
                                                AppRole,
                                                int,
                                                IdentityUserClaim<int>,
                                                AppUserRole,
                                                IdentityUserLogin<int>,
                                                IdentityRoleClaim<int>,
                                                IdentityUserToken<int>>
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Accounts { get; set; }
        public DbSet<AppRole> Roles { get; set; }
        public DbSet<AppUserRole> AppUserRole { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MilkAge> MilkAges { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsageHistory> CouponUsageHistories { get; set; }
        public DbSet<Milk> Milks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }

        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                 .AddJsonFile("MomMomMilks/appsettings.json", true, true)
                 .Build();
            var strConn = config["ConnectionStrings:DefaultConnectionString"];

            return strConn;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId);


            builder.Entity<AppUser>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.AppUser)
                .HasForeignKey(u => u.UserId)
                .IsRequired();


            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(ur => ur.AppRole)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();


            builder.Entity<Brand>()
                .HasKey(b => b.Id);


            builder.Entity<Category>()
                .HasKey(c => c.Id);


            builder.Entity<Coupon>()
                .HasKey(c => c.Id);


            builder.Entity<CouponUsageHistory>()
                .HasKey(k => new { k.OrderId, k.CouponId });
            builder.Entity<CouponUsageHistory>()
                .HasOne(cuh => cuh.Coupon)
                .WithMany(c => c.CouponUsageHistories)
                .HasForeignKey(cuh => cuh.CouponId);
            builder.Entity<CouponUsageHistory>()
                .HasOne(cuh => cuh.Order)
                .WithMany(o => o.CouponUsageHistories)
                .HasForeignKey(cuh => cuh.OrderId);


            builder.Entity<Order>()
                .HasKey(or => or.Id);
            builder.Entity<Order>()
                .HasOne(or => or.PaymentType)
                .WithMany(pt => pt.Orders)
                .HasForeignKey(or => or.PaymentTypeId);
            builder.Entity<Order>()
                .HasOne(or => or.Address)
                .WithMany(a => a.Orders)
                .HasForeignKey(or => or.AddressId);
            builder.Entity<Order>()
                .HasOne(or => or.Buyer)
                .WithMany(b => b.Orders)
                .HasForeignKey(or => or.BuyerId);
            builder.Entity<Order>()
                .HasOne(or => or.Transaction)
                .WithOne(ts => ts.Order)
                .HasForeignKey<Order>(or => or.TransactionId);


            builder.Entity<Report>()
                .HasKey(r => r.Id);
            builder.Entity<Report>()
                .HasOne(r => r.Order)
                .WithMany(or => or.Reports)
                .HasForeignKey(r => r.OrderId);
            builder.Entity<Report>()
                .HasOne(r => r.Staff)
                .WithMany(s => s.Reports)
                .HasForeignKey(r => r.StaffId);


            builder.Entity<OrderDetail>()
                .HasKey(od => od.Id);
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Milk)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.MilkId);


            builder.Entity<Feedback>()
                .HasKey(f => f.Id);
            builder.Entity<Feedback>()
                .HasOne(f => f.Milk)
                .WithMany(m => m.Feedbacks)
                .HasForeignKey(f => f.MilkId);
            builder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId);


            builder.Entity<Article>()
                .HasKey(a => a.Id);
            builder.Entity<Article>()
                .HasOne(a => a.AppUser)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.UserId);
            builder.Entity<Article>()
                .HasOne(a => a.Milk)
                .WithMany(m => m.Articles)
                .HasForeignKey(a => a.MilkId);


            builder.Entity<Milk>()
                .HasOne(m => m.Brand)
                .WithMany(b => b.Milks)
                .HasForeignKey(m => m.BrandId);
            builder.Entity<Milk>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Milks)
                .HasForeignKey(m => m.CategoryId);
            builder.Entity<Milk>()
                .HasOne(m => m.MilkAge)
                .WithMany(a => a.Milks)
                .HasForeignKey(m => m.MilkAgeId);
            builder.Entity<Milk>()
                .HasOne(m => m.Supplier)
                .WithMany(s => s.Milks)
                .HasForeignKey(m => m.SupplierId);


            builder.Entity<MilkAge>()
                .HasKey(ma => ma.Id);


            builder.Entity<PaymentType>()
                .HasKey(pt => pt.Id);


            builder.Entity<Schedule>()
                .HasKey(s => new { s.ShipperId, s.OrderId });
            builder.Entity<Schedule>()
                .HasOne(s => s.Shipper)
                .WithMany(sh => sh.Schedules)
                .HasForeignKey(s => s.ShipperId);
            builder.Entity<Schedule>()
                .HasOne(s => s.Order)
                .WithOne(o => o.Schedule)
                .HasForeignKey<Schedule>(s => s.OrderId);


            builder.Entity<Shipper>()
                .HasKey(s => s.Id);
            builder.Entity<Shipper>()
                .HasOne(s => s.AppUser)
                .WithOne(au => au.Shipper)
                .HasForeignKey<Shipper>(s => s.AppUserId);
                


            builder.Entity<Supplier>()
                .HasKey(s => s.Id);
        }
    }
}