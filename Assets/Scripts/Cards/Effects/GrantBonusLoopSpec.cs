using System;
using UnityEngine;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// [R5-d / D-R0-5=A] Grants ONE bonus loop of the currently running part,
    /// optionally with a one-loop soloist track layered over the base.
    ///
    /// The spec carries NO cost. Under D-R5-26=A the resource price lives on
    /// <see cref="ALWTTT.Cards.CardDefinition"/> (resourceCostStatusKey +
    /// resourceCostAmount) so it can be checked and spent BEFORE the play is
    /// committed. A cost inside an effect spec would run after the inspiration
    /// and the ECON-1 budget were already burned, which would let the card be
    /// played with an empty resource and then fail silently.
    ///
    /// Runtime owner: GigManager.TryGrantBonusLoop → CompositionSession.
    /// Dispatched from CardBase.ExecuteEffects.
    /// </summary>
    [Serializable]
    public sealed class GrantBonusLoopSpec : CardEffectSpec
    {
        [Tooltip("When on, the bonus loop is rendered with an extra soloist " +
                 "track layered over the unchanged base (render scope only — " +
                 "the composition model is never mutated). When off, the bonus " +
                 "loop is a plain repeat of the part.")]
        public bool soloOverBonusLoop = true;
    }
}