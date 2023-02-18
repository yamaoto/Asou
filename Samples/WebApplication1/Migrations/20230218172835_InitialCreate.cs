using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSubscriptionModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessInstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ThreadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    EventSubscriptionType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSubscriptionModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessExecutionLogModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcessContractId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessInstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ThreadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessExecutionLogModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessInstanceModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessContractId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    PersistenceType = table.Column<byte>(type: "INTEGER", nullable: false),
                    State = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessInstanceModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessParameterPersistentModel",
                columns: table => new
                {
                    ProcessInstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JsonContent = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessParameterPersistentModel", x => x.ProcessInstanceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSubscriptionModel");

            migrationBuilder.DropTable(
                name: "ProcessExecutionLogModel");

            migrationBuilder.DropTable(
                name: "ProcessInstanceModel");

            migrationBuilder.DropTable(
                name: "ProcessParameterPersistentModel");
        }
    }
}
