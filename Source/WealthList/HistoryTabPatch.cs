﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace YinMu.Source.WealthList
{
    [Harmony]
    internal class HistoryTabPatch
    {
        private static readonly List<Thing> tmpThings = [];
        private static List<WealthThingGroup> DisplayGroup = [];
        private static Vector2 scrollPos = Vector2.one;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.PreOpen))]
        private static void PreOpen() => RefreshThings();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainTabWindow_History), "DoStatisticsPage")]
        private static void DoStatisticsPage(Rect rect) => DoWealthList(rect);

        private static void RefreshThings()
        {
            tmpThings.Clear();
            DisplayGroup.Clear();
            var map = Find.CurrentMap;
            //物品
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), tmpThings, false, WealthWatcher.WealthItemsFilter);//源码写法
            var list = tmpThings.Where(t => t.SpawnedOrAnyParentSpawned && !t.PositionHeld.Fogged(Find.CurrentMap) && t.MarketValue > 0).ToList();
            DisplayGroup.Add(new WealthThingGroup("ThisMapColonyWealthItems".Translate(), list, t => t.MarketValue * t.stackCount));
            tmpThings.Clear();
            //建筑艺术
            list = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(b => b.Faction == Faction.OfPlayer).ToList();
            DisplayGroup.Add(new WealthThingGroup("ThisMapColonyWealthBuildings".Translate(), list, t => t.stackCount * t.GetStatValue(StatDefOf.MarketValueIgnoreHp)));
            //人和动物
            list = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => !p.IsQuestLodger()).ToList<Thing>();
            DisplayGroup.Add(new WealthThingGroup("ThisMapColonyWealthColonistsAndTameAnimals".Translate(), list, t =>
            {
                if (t is Pawn p && p.IsSlave)
                {
                    //奴隶减价
                    return p.MarketValue * 0.75f;
                }
                return t.MarketValue;
            }));
        }

        public static void DoWealthList(Rect inRect)
        {
            inRect.xMin += 400f;
            var listing = new Listing_Standard();
            //滚动界面
            var viewRect = new Rect(0, 0, inRect.width - 30f, DisplayGroup.Count * 35f + DisplayGroup.Where(item => item.expanded).SelectMany(item => item.things).Count() * 33f);
            var anchor = Text.Anchor;
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
            listing.Begin(viewRect);

            var highlight = false;

            foreach (var group in DisplayGroup)
            {
                if (group.things.Count < 1) continue;

                var groupRect = listing.GetRect(30f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(groupRect.LeftHalf().RightPart(0.9f), group.name);
                if (Widgets.ButtonImage(groupRect.LeftPartPixels(30f), group.expanded ? TexButton.Collapse : TexButton.Reveal)) group.expanded = !group.expanded;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(groupRect.RightHalf().LeftPart(0.8f), group.Wealth.ToStringMoney());
                group.Desc();//排序
                highlight = !highlight;
                if (group.expanded)
                {
                    listing.Gap(3f);//上下间隔
                    listing.Indent();//缩进
                    listing.ColumnWidth -= 12f;
                    foreach (var wealthThing in group.things)
                    {
                        var rect = listing.GetRect(30f);
                        if (highlight) Widgets.DrawLightHighlight(rect);
                        if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);
                        Widgets.ThingIcon(rect.LeftPartPixels(50f).RightPartPixels(30f), wealthThing.thing);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(rect.LeftHalf().RightPart(0.8f), wealthThing.DisplayName);
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(rect.RightHalf().LeftPart(0.8f), wealthThing.wealth.ToStringMoney());
                        if (Widgets.ButtonInvisible(rect))
                        {
                            DefDatabase<MainButtonDef>.GetNamed("History").TabWindow.Close();
                            CameraJumper.TryJumpAndSelect(wealthThing.thing);
                        }

                        listing.Gap(2f);
                        highlight = !highlight;
                    }

                    listing.ColumnWidth += 12f;
                    listing.Outdent();
                }

                listing.Gap(5f);
            }

            Text.Anchor = anchor;
            listing.End();
            Widgets.EndScrollView();
        }
    }
}