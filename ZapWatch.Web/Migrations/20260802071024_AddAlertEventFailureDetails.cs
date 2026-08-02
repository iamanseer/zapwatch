using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZapWatch.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertEventFailureDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureDetails",
                table: "AlertEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureDetails",
                table: "AlertEvents");
        }
    }
}
