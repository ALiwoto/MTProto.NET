﻿// <auto-generated />
using MTProto.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MTProto.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221203111054_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("MTProto.Core.Database.Models.OwnerPeerInfo", b =>
                {
                    b.Property<long>("OwnerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBot")
                        .HasColumnType("INTEGER");

                    b.HasKey("OwnerId");

                    b.ToTable("OwnerInfos");
                });

            modelBuilder.Entity("MTProto.Core.Database.Models.PeerInfo", b =>
                {
                    b.Property<long>("PeerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessHash")
                        .HasColumnType("TEXT");

                    b.Property<int>("PeerType")
                        .HasColumnType("INTEGER");

                    b.HasKey("PeerId");

                    b.ToTable("PeerInfos");
                });
#pragma warning restore 612, 618
        }
    }
}
