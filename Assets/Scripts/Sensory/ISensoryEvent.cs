namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S2 D-S2-4=A] Marker interface for events published on the
    /// <see cref="SensoryEventBus"/>.
    ///
    /// Event types are immutable <c>readonly struct</c>s implementing this
    /// interface. The bus is generic (<c>Publish&lt;TEvent&gt; where TEvent :
    /// ISensoryEvent</c>), so structs are never boxed and publishes are
    /// allocation-free. The marker exists purely so the compiler rejects
    /// arbitrary types on the bus.
    ///
    /// Events carry SEMANTIC payloads only (what happened, to whom, with what
    /// magnitudes). Presentation mapping (FT text, color, drift) lives in
    /// SensoryFtPresentation and in future S3 SFX consumers — never in the
    /// event itself. This is what lets one event feed FT + SFX + animator
    /// consumers without shape changes (Standing Directive D1).
    /// </summary>
    public interface ISensoryEvent { }
}