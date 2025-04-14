using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogProducer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLogModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostId",
                table: "LogMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Application",
                table: "LogMessages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Application",
                table: "LogMessages",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostId",
                table: "LogMessages",
                type: "text",
                nullable: true);
        }
    }
}
