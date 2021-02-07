using StoryMode;
using StoryMode.Behaviors.Quests.FirstPhase;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Reflection;
using System;

namespace ResetMainStory
{
    public class ResetMainStorySubModule : MBSubModuleBase
    {
        private Game game;
        private Campaign campaign;
        private bool once;

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            if (game.GameType is CampaignStoryMode)
            {
                this.game = game;
                this.campaign = game.GameType as Campaign;
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            if (game != null && !once) {
                once = true;
                var bannerInvestigationQuestType = typeof(BannerInvestigationQuestBehavior).GetNestedType(
                    "BannerInvestigationQuest",
                    BindingFlags.NonPublic
                );

                var storyModeCurrent = StoryMode.StoryMode.Current;
                var ctor = bannerInvestigationQuestType.GetConstructor(new Type[] { typeof(Hero) });
                var elderBrotherProperty = typeof(StoryModeHeroes).GetProperty("ElderBrother", BindingFlags.NonPublic | BindingFlags.Static);
                var elderBrother = elderBrotherProperty.GetValue(null);

                var quest = ctor.Invoke(new object[] { elderBrother }) as QuestBase;
                quest.StartQuest();

                var mapTimeTrackerProperty = typeof(Campaign).GetProperty("MapTimeTracker", BindingFlags.NonPublic | BindingFlags.Instance);
                var mapTimeTracker = mapTimeTrackerProperty.GetValue(campaign);

                var campaignTimeNumTicksField = typeof(CampaignTime).GetField("_numTicks", BindingFlags.NonPublic | BindingFlags.Instance);
                var campaignStartTimeNumTicks = campaignTimeNumTicksField.GetValue(CampaignData.CampaignStartTime);

                var mapTimeTrackerNumTicksField = mapTimeTracker.GetType().GetField("_numTicks", BindingFlags.NonPublic | BindingFlags.Instance);
                mapTimeTrackerNumTicksField.SetValue(mapTimeTracker, campaignStartTimeNumTicks);

                var mapTimeTrackerNowProperty = mapTimeTracker.GetType().GetProperty("Now", BindingFlags.NonPublic | BindingFlags.Instance);
                var mapTimeTrackerNow = mapTimeTrackerNowProperty.GetValue(mapTimeTracker);

                var campaignStartTimeProperty = campaign.GetType().GetProperty("CampaignStartTime").DeclaringType.GetProperty("CampaignStartTime");
                campaignStartTimeProperty.SetValue(campaign, mapTimeTrackerNow);

            }
        }
    }
}
