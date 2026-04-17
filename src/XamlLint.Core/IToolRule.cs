namespace XamlLint.Core;

/// <summary>
/// Marker interface on tool/engine diagnostic rules (LX001-LX099). The rule dispatcher skips
/// these — their diagnostics are emitted directly by the pipeline site that detected the
/// condition (malformed XAML, bad pragma, config error, etc.). They exist as
/// <see cref="IXamlRule"/> implementations solely to register their IDs with the generated
/// catalog so meta-tests, docs, schema, and presets include them.
/// </summary>
public interface IToolRule : IXamlRule { }
