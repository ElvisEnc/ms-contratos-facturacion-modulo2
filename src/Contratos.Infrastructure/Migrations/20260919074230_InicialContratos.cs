using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contratos.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InicialContratos : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "contratos",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                servicio_tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                servicio_nombre = table.Column<string>(type: "text", nullable: false),
                servicio_precio_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                servicio_precio_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                fecha_contratacion = table.Column<DateOnly>(type: "date", nullable: false),
                vigencia_fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                vigencia_fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                vigencia_duracion_dias = table.Column<int>(type: "integer", nullable: false),
                estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                pago_fecha = table.Column<DateOnly>(type: "date", nullable: true),
                pago_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                pago_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                pago_referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_contratos", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "controles_adicionales",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                evaluacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                fecha = table.Column<DateOnly>(type: "date", nullable: false),
                cobro_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                cobro_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                pagado = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_controles_adicionales", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "facturas",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                referencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                fecha = table.Column<DateOnly>(type: "date", nullable: false),
                subtotal_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                subtotal_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                impuesto_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                impuesto_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                total_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                total_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_facturas", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "pacientes_conocidos",
            columns: table => new
            {
                paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                nombre = table.Column<string>(type: "text", nullable: false),
                nro_identificacion = table.Column<string>(type: "text", nullable: false),
                plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                tiene_plan_activo = table.Column<bool>(type: "boolean", nullable: false),
                plan_fecha_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                plan_duracion_dias = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pacientes_conocidos", x => x.paciente_id);
            });

        migrationBuilder.CreateTable(
            name: "servicios_catalogo",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                nombre = table.Column<string>(type: "text", nullable: false),
                precio_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                precio_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                activo = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_servicios_catalogo", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "factura_lineas",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                concepto = table.Column<string>(type: "text", nullable: false),
                cantidad = table.Column<int>(type: "integer", nullable: false),
                precio_unitario_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                precio_unitario_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                factura_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_factura_lineas", x => x.id);
                table.ForeignKey(
                    name: "FK_factura_lineas_facturas_factura_id",
                    column: x => x.factura_id,
                    principalTable: "facturas",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_contratos_fecha_contratacion",
            table: "contratos",
            column: "fecha_contratacion");

        migrationBuilder.CreateIndex(
            name: "ix_contratos_paciente_id",
            table: "contratos",
            column: "paciente_id");

        migrationBuilder.CreateIndex(
            name: "ux_contratos_catering_activo_por_paciente",
            table: "contratos",
            column: "paciente_id",
            unique: true,
            filter: "servicio_tipo = 'CATERING' AND estado = 'ACTIVO'");

        migrationBuilder.CreateIndex(
            name: "ix_controles_adicionales_paciente_id",
            table: "controles_adicionales",
            column: "paciente_id");

        migrationBuilder.CreateIndex(
            name: "ux_controles_evaluacion_id",
            table: "controles_adicionales",
            column: "evaluacion_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_factura_lineas_factura_id",
            table: "factura_lineas",
            column: "factura_id");

        migrationBuilder.CreateIndex(
            name: "ix_facturas_paciente_id",
            table: "facturas",
            column: "paciente_id");

        migrationBuilder.CreateIndex(
            name: "ix_facturas_referencia_id",
            table: "facturas",
            column: "referencia_id");

        migrationBuilder.CreateIndex(
            name: "ux_servicios_catalogo_tipo_activo",
            table: "servicios_catalogo",
            column: "tipo",
            unique: true,
            filter: "activo = true");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "contratos");

        migrationBuilder.DropTable(
            name: "controles_adicionales");

        migrationBuilder.DropTable(
            name: "factura_lineas");

        migrationBuilder.DropTable(
            name: "pacientes_conocidos");

        migrationBuilder.DropTable(
            name: "servicios_catalogo");

        migrationBuilder.DropTable(
            name: "facturas");
    }
}
