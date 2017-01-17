﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvoNet.Controls;
using Graph;
using EvoNet.Map;
using EvoNet.Objects;

namespace EvoNet.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            numberCreaturesAlive.Color = Color.Red;
            foodValueList.Color = Color.Green;
            NumberOfCreaturesAliveGraph.Add("Creatures", numberCreaturesAlive);
            NumberOfCreaturesAliveGraph.Add("Food", foodValueList);
            evoSimControl1.OnUpdate += EvoSimControl1_OnUpdate;

        }

        private void EvoSimControl1_OnUpdate(Microsoft.Xna.Framework.GameTime obj)
        {
            string status = "#: {0} D: {1} max(G): {2} Y: {3} LS: {4} LSA: {5} AvgDA: {6}";
            status = string.Format(
                status,
                CreatureManager.Creatures.Count,
                CreatureManager.numberOfDeaths,
                Creature.maximumGeneration,
                CreatureManager.year,
                Creature.oldestCreatureEver != null ?
                    Creature.oldestCreatureEver.Age :
                    0,
                CreatureManager.OldestCreatureAlive != null ?
                    CreatureManager.OldestCreatureAlive.Age :
                    0,
                CreatureManager.CalculateAverageAgeOfLastDeadCreatures());
            toolStripStatusLabel1.Text = status;
        }

        GraphValueList foodValueList = new GraphValueList();
        GraphValueList numberCreaturesAlive = new GraphValueList();
        int lastFoodIndex = 0;
        int lastCreatureIndex = 0;

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();

        }

        private TileMap TileMap
        {
            get
            {
                return evoSimControl1.sim.TileMap;
            }
        }

        private CreatureManager CreatureManager
        {
            get
            {
                return evoSimControl1.sim.CreatureManager;
            }
        }

        DateTime fictionalDateForFood = DateTime.Now;
        DateTime fictionalDateForCreatures = DateTime.Now;

        private void timer1_Tick(object sender, EventArgs e)
        {
            while (lastFoodIndex < TileMap.FoodRecord.Count)
            {
                fictionalDateForFood += TimeSpan.FromSeconds(TileMap.FixedUpdateTime);
                float Value = TileMap.FoodRecord[lastFoodIndex];
                lastFoodIndex++;
                foodValueList.Add(new GraphTimeDoubleValue(fictionalDateForFood, Value));
            }
            while (lastCreatureIndex < CreatureManager.AliveCreaturesRecord.Count)
            {
                fictionalDateForCreatures += TimeSpan.FromSeconds(CreatureManager.FixedUpdateTime);
                float Value = CreatureManager.AliveCreaturesRecord[lastCreatureIndex];
                lastCreatureIndex++;
                numberCreaturesAlive.Add(new GraphTimeDoubleValue(fictionalDateForCreatures, Value));
            }
            //FoodGraph.Refresh();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            evoSimControl1.Serialize(true);
        }

        private void showStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
            showStatisticsToolStripMenuItem.Checked = !splitContainer1.Panel2Collapsed;
        }
    }
}
