using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace YinMu.Source.ShowTrade
{
    internal class Dialog_TradeableGoods : Window
    {
        protected static readonly Vector2 ButtonSize = new Vector2(160f, 40f);

        private Vector2 scrollPosition = Vector2.zero;
        private ITrader trader;
        private Settlement settlement;
        private List<TradeableGroup> displayGroup = [];
        private Tradeable currencyTradeable;//商队持有的金钱
        private Pawn negotiator;
        private readonly float negotiatorStat;
        public override Vector2 InitialSize => new Vector2(1024f * 0.8f, UI.screenHeight);

        public Dialog_TradeableGoods(ITrader trader, Pawn negotiator)
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            this.trader = trader;
            this.negotiator = negotiator;
            settlement = (Settlement)trader;
            negotiatorStat = StatExtension.GetStatValue(negotiator, StatDefOf.TradePriceImprovement);
            TradeSession.trader = trader;
            TradeSession.giftMode = false;
            TradeSession.playerNegotiator = negotiator;
            Group();
        }

        public override void DoWindowContents(Rect inRect)
        {
            float num = 0;//判断高度
            //显示标题：派系和下一次刷新时间
            Text.Font = GameFont.Medium;//Medium:中等的
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0, 0, inRect.width, 50f), "WindowTitle".Translate(settlement.Name, (settlement.trader.NextRestockTick - Find.TickManager.TicksGame).ToStringTicksToDays("F1")));
            num += 50f;
            //显示最佳交易小人
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect = new Rect(0f, num + 10f, inRect.width, 40f);
            Widgets.ThingIcon(rect.LeftPartPixels(40), negotiator);
            Widgets.Label(rect.RightPartPixels(inRect.width - 40f), "BestPawn".Translate(negotiator.NameFullColored, negotiatorStat.ToStringPercent()));
            num += 50f;
            //显示交易对所持有的金钱,-16:下方滑轮占16
            DrawTradeableRow(new Rect(0f, num, inRect.width - 16f, 30f), currencyTradeable, 0);
            num += 30f;
            //主要内容
            Widgets.DrawLineHorizontal(0, num, inRect.width);
            rect = new Rect(0f, num + 10f, inRect.width, inRect.height - num - 80f);
            FillMainRect(rect);
            //关闭按钮
            if (Widgets.ButtonText(new Rect(inRect.width / 2f - ButtonSize.x / 2f, rect.y + rect.height, ButtonSize.x, ButtonSize.y), "CloseButton".Translate()))
            {
                Close();
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void FillMainRect(Rect rect)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, displayGroup.Sum(g =>
             g.expanded ? g.list.Count + 1 : 1) * 30f);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float num = 0;
            foreach (var group in displayGroup)
            {
                int index = 1;
                var groupRect = new Rect(0f, num, viewRect.width, 30f);
                Widgets.Label(groupRect.LeftHalf().RightPart(0.9f), group.name);
                if (Widgets.ButtonImage(groupRect.LeftPartPixels(30f), group.expanded ? TexButton.Collapse : TexButton.Reveal)) group.expanded = !group.expanded;
                num += 30f;
                if (group.expanded)
                {
                    foreach (var item in group.list)
                    {
                        DrawTradeableRow(new Rect(0f, num, viewRect.width, 30f), item, index);
                        num += 30f;
                        index++;
                    }
                }
            }
            Widgets.EndScrollView();
        }

        private void DrawTradeableRow(Rect rect, Tradeable trad, int index)

        {
            if (trad == null) return;

            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Text.Font = GameFont.Small;
            Widgets.BeginGroup(rect);
            float width = rect.width;
            int num = trad.CountHeldBy(Transactor.Trader);
            Rect rect2 = new Rect(width - 75f, 0f, 75f, rect.height);
            if (num != 0 && trad.IsThing)
            {
                if (Mouse.IsOver(rect2)) Widgets.DrawHighlight(rect2);

                Text.Anchor = TextAnchor.MiddleRight;
                Rect rect3 = rect2;
                rect3.xMin += 5f;
                rect3.xMax -= 5f;
                Widgets.Label(rect3, num.ToStringCached());//持有的数量
                TooltipHandler.TipRegionByKey(rect2, "TraderCount");
                Rect rect4 = new Rect(rect2.x - 100f, 0f, 100f, rect.height);
                DrawPrice(rect4, trad);
            }
            width /= 2f;
            TransferableUIUtility.DoExtraIcons(trad, rect, ref width);
            if (ModsConfig.IdeologyActive)
            {
                TransferableUIUtility.DrawCaptiveTradeInfo(trad, TradeSession.trader, rect, ref width);
            }
            TransferableUIUtility.DrawTransferableInfo(trad, new Rect(0f, 0f, rect.width * 0.6f, rect.height), Color.white);
            GenUI.ResetLabelAlign();
            Widgets.EndGroup();
        }

        private void DrawPrice(Rect rect, Tradeable trad)
        {
            rect = rect.Rounded();
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, new TipSignal(() => trad.GetPriceTooltip(TradeAction.PlayerBuys), trad.GetHashCode() * 297));
            }
            Rect rect2 = new Rect(rect);
            rect2.xMax -= 5f;
            rect2.xMin -= 295f;

            Widgets.Label(rect2, trad.GetPriceFor(TradeAction.PlayerBuys).ToStringMoney());
        }

        private void Group()
        {
            List<Tradeable> tradeables = [];
            //获取交易物品
            foreach (var good in trader.Goods)
            {
                Tradeable tradeable = TransferableUtility.TradeableMatching(good, tradeables);
                if (tradeable == null)
                {
                    tradeable = !(good is Pawn) ? new Tradeable() : new Tradeable_Pawn();
                    tradeables.Add(tradeable);
                }
                tradeable.AddThing(good, Transactor.Trader);
            }
            //Currency:货币
            currencyTradeable = GenCollection.FirstOrDefault<Tradeable>(tradeables, x => x.IsCurrency && !x.IsFavor);
            tradeables = tradeables.Where(tr => !tr.IsCurrency && (tr.TraderWillTrade || !TradeSession.trader.TraderKind.hideThingsNotWillingToTrade)).ToList();
            //对物品进行分组
            displayGroup.Add(new TradeableGroup
            {
                name = "PawnsTabShort".Translate(),//生物
                list = tradeables.Where(tr => tr is Tradeable_Pawn).ToList(),
            });
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.Foods, tradeables));//食物
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.ResourcesRaw, tradeables));//原材料
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.Weapons, tradeables));//武器
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.Items, tradeables));//物品
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.Manufactured, tradeables));//制成品
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.Apparel, tradeables));//衣物
            displayGroup.Add(new TradeableGroup(ThingCategoryDefOf.Buildings, tradeables));//建筑

            displayGroup.RemoveWhere(g => g.list.Count < 1);
            //TODO:排序
        }
    }
}