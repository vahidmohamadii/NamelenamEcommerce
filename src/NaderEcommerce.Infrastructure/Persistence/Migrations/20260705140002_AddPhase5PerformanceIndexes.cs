using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaderEcommerce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase5PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteSettings_CreatedAt",
                table: "WebsiteSettings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Sliders_IsActive_DisplayOrder",
                table: "Sliders",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsApproved_CreatedAt",
                table: "Reviews",
                columns: new[] { "IsApproved", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QRLinks_IsActive",
                table: "QRLinks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_CreatedAt",
                table: "Products",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_IsBestSeller_CreatedAt",
                table: "Products",
                columns: new[] { "IsActive", "IsBestSeller", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_IsFeatured_CreatedAt",
                table: "Products",
                columns: new[] { "IsActive", "IsFeatured", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsPrimary_DisplayOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsPrimary", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_VerifiedAt",
                table: "Payments",
                columns: new[] { "Status", "VerifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_IsPublished_Key",
                table: "Pages",
                columns: new[] { "IsPublished", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_IsPublished_Slug",
                table: "Pages",
                columns: new[] { "IsPublished", "Slug" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_IsActive_StartsAt_EndsAt",
                table: "Coupons",
                columns: new[] { "IsActive", "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive_SortOrder",
                table: "Categories",
                columns: new[] { "IsActive", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WebsiteSettings_CreatedAt",
                table: "WebsiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Sliders_IsActive_DisplayOrder",
                table: "Sliders");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_IsApproved_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_QRLinks_IsActive",
                table: "QRLinks");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_IsBestSeller_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_IsFeatured_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_IsPrimary_DisplayOrder",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_VerifiedAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Pages_IsPublished_Key",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_IsPublished_Slug",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_IsActive_StartsAt_EndsAt",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IsActive_SortOrder",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");
        }
    }
}
