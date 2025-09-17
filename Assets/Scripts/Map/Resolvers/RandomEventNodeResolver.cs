using ALWTTT.Data;
using ALWTTT.Enums; // NodeType
using ALWTTT.Events;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Map
{
    public class RandomEventNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.RandomEvent;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            RandomEventOption chosen = null;
            yield return ctx.ShowRandomEvent(o => chosen = o);

            if (chosen == null)
                yield break; // player closed or nothing chosen

            ApplyOption(chosen, ctx);

            //ctx.Persistent.MarkRandomEventUsed(eventData.EventId, persistent.CurrentSectorId);

            node.Completed = true;
            ctx.RefreshHUD();
        }

        private void ApplyOption(RandomEventOption option, NodeResolveContext ctx)
        {
            var pd = ctx.Persistent;
            var gp = GameManager.Instance.GameplayData; // for max cohesion, etc. (if needed)

            foreach (var fx in option.effects)
            {
                switch (fx.type)
                {
                    case RandomEventEffectType.GainFans:
                        pd.Fans += fx.amount;
                        break;

                    case RandomEventEffectType.ChangeCohesion:
                        pd.BandCohesion = Mathf.Clamp(
                            pd.BandCohesion + fx.amount,
                            0, gp.MaxCohesion);
                        break;

                    case RandomEventEffectType.AddCard:
                        if (fx.card != null)
                            pd.AddCardToDeck(fx.card);
                        break;

                    case RandomEventEffectType.AddCards:
                        if (fx.cards != null)
                            foreach (var c in fx.cards)
                                if (c) pd.AddCardToDeck(c);
                        break;

                    case RandomEventEffectType.AddMusician:
                        if (fx.musician != null)
                            pd.AddMusicianToBand(fx.musician);
                        break;

                    case RandomEventEffectType.RemoveMusician:
                        if (!string.IsNullOrEmpty(fx.musicianId))
                            pd.RemoveMusicianFromBand(fx.musicianId);
                        break;

                    case RandomEventEffectType.AddStoryTag:
                        if (!string.IsNullOrEmpty(fx.storyTag))
                            pd.AddStoryTag(fx.storyTag); // implement if missing
                        break;
                }
            }
        }
    }
}
