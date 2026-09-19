namespace Contratos.Domain.Shared;

public sealed record ServicioCatalogoId
{
    public Guid Value { get; }

    private ServicioCatalogoId(Guid value) => Value = value;

    public static ServicioCatalogoId New() => new(Guid.NewGuid());

    public static ServicioCatalogoId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new ServicioCatalogoId(value);
    }
}

public sealed record ContratoId
{
    public Guid Value { get; }

    private ContratoId(Guid value) => Value = value;

    public static ContratoId New() => new(Guid.NewGuid());

    public static ContratoId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new ContratoId(value);
    }
}

public sealed record FacturaId
{
    public Guid Value { get; }

    private FacturaId(Guid value) => Value = value;

    public static FacturaId New() => new(Guid.NewGuid());

    public static FacturaId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new FacturaId(value);
    }
}

public sealed record LineaFacturaId
{
    public Guid Value { get; }

    private LineaFacturaId(Guid value) => Value = value;

    public static LineaFacturaId New() => new(Guid.NewGuid());

    public static LineaFacturaId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new LineaFacturaId(value);
    }
}

public sealed record ControlId
{
    public Guid Value { get; }

    private ControlId(Guid value) => Value = value;

    public static ControlId New() => new(Guid.NewGuid());

    public static ControlId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new ControlId(value);
    }
}

public sealed record PacienteId
{
    public Guid Value { get; }

    private PacienteId(Guid value) => Value = value;

    public static PacienteId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new PacienteId(value);
    }
}

public sealed record EvaluacionId
{
    public Guid Value { get; }

    private EvaluacionId(Guid value) => Value = value;

    public static EvaluacionId From(Guid value)
    {
        TypedIdValidator.ValidarNoVacio(value);
        return new EvaluacionId(value);
    }
}
