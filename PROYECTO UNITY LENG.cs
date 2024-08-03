using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Text;

namespace RockGodTycoon.Tests
{
    public class RockGodTycoonFlowIntegrationTests
    {
        private RockGodTycoonFlowIntegration flowIntegration;
        private MockFlowClient mockFlowClient;
        private MockBlockchain mockBlockchain;

        [SetUp]
        public void Setup()
        {
            // Create a new instance of the class under test
            flowIntegration = new GameObject().AddComponent<RockGodTycoonFlowIntegration>();

            // Create mock dependencies
            mockFlowClient = new MockFlowClient();
            mockBlockchain = new MockBlockchain();

            // Set the mock dependencies on the class under test
            flowIntegration.flowClient = mockFlowClient;
            flowIntegration.blockchain = mockBlockchain;

            // Initialize UI elements
            flowIntegration.moneyText = new GameObject().AddComponent<Text>();
            flowIntegration.fansText = new GameObject().AddComponent<Text>();
            flowIntegration.levelText = new GameObject().AddComponent<Text>();
            flowIntegration.studioLevelText = new GameObject().AddComponent<Text>();
            flowIntegration.songQualityText = new GameObject().AddComponent<Text>();
            flowIntegration.tourCapacityText = new GameObject().AddComponent<Text>();
            flowIntegration.tourCostText = new GameObject().AddComponent<Text>();
            flowIntegration.songCostText = new GameObject().AddComponent<Text>();
            flowIntegration.studioUpgradeCostText = new GameObject().AddComponent<Text>();
            flowIntegration.tourUpgradeCostText = new GameObject().AddComponent<Text>();
            flowIntegration.songUpgradeCostText = new GameObject().AddComponent<Text>();
            flowIntegration.errorPanel = new GameObject();
            flowIntegration.errorText = new GameObject().AddComponent<Text>();

            // Initialize game data
            flowIntegration.currentMoney = 100;
            flowIntegration.currentFans = 0;
            flowIntegration.currentLevel = 1;
            flowIntegration.currentStudioLevel = 1;
            flowIntegration.currentSongQuality = 1;
            flowIntegration.currentTourCapacity = 10;
            flowIntegration.currentTourCost = 50;
            flowIntegration.currentSongCost = 20;
            flowIntegration.currentStudioUpgradeCost = 100;
            flowIntegration.currentTourUpgradeCost = 200;
            flowIntegration.currentSongUpgradeCost = 400;
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy the game object
            Destroy(flowIntegration.gameObject);
        }

        [Test]
        public void SavePlayerData_ShouldSaveDataToFlowStorageAndBlockchain()
        {
            // Arrange
            string expectedJsonData = JsonUtility.ToJson(new RockGodTycoonFlowIntegration.PlayerData(
                flowIntegration.currentMoney, flowIntegration.currentFans, flowIntegration.currentLevel, flowIntegration.currentStudioLevel,
                flowIntegration.currentSongQuality, flowIntegration.currentTourCapacity, flowIntegration.currentTourCost,
                flowIntegration.currentSongCost, flowIntegration.currentStudioUpgradeCost, flowIntegration.currentTourUpgradeCost,
                flowIntegration.currentSongUpgradeCost
            ));

            // Act
            flowIntegration.SavePlayerData();

            // Assert
            Assert.AreEqual(expectedJsonData, mockFlowClient.SavedPlayerData);
            Assert.AreEqual(1, mockBlockchain.Blocks.Count);
            Assert.AreEqual(flowIntegration.currentMoney, mockBlockchain.Blocks[0].Transactions[0].Amount);
        }

        [Test]
        public void LoadPlayerData_ShouldLoadFromFlowStorageAndBlockchain()
        {
            // Arrange
            string jsonData = JsonUtility.ToJson(new RockGodTycoonFlowIntegration.PlayerData(
                200, 100, 2, 2, 2, 20, 100, 40, 200, 400, 800
            ));
            mockFlowClient.LoadedPlayerData = jsonData;
            mockFlowClient.USDCBalance = 200;

            // Act
            flowIntegration.LoadPlayerData();

            // Assert
            Assert.AreEqual(200, flowIntegration.currentMoney);
            Assert.AreEqual(100, flowIntegration.currentFans);
            Assert.AreEqual(2, flowIntegration.currentLevel);
            Assert.AreEqual(2, flowIntegration.currentStudioLevel);
            Assert.AreEqual(2, flowIntegration.currentSongQuality);
            Assert.AreEqual(20, flowIntegration.currentTourCapacity);
            Assert.AreEqual(100, flowIntegration.currentTourCost);
            Assert.AreEqual(40, flowIntegration.currentSongCost);
            Assert.AreEqual(200, flowIntegration.currentStudioUpgradeCost);
            Assert.AreEqual(400, flowIntegration.currentTourUpgradeCost);
            Assert.AreEqual(800, flowIntegration.currentSongUpgradeCost);
        }

        [Test]
        public void OnRecordSong_ShouldRecordSongIfSufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentSongCost;

            // Act
            flowIntegration.OnRecordSong();

            // Assert
            Assert.AreEqual(flowIntegration.currentMoney - flowIntegration.currentSongCost, flowIntegration.currentMoney);
            Assert.AreEqual(flowIntegration.currentSongQuality + 1, flowIntegration.currentSongQuality);
            Assert.AreEqual(flowIntegration.currentSongCost * 2, flowIntegration.currentSongCost);
        }

        [Test]
        public void OnRecordSong_ShouldShowErrorIfInsufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentSongCost - 1;

            // Act
            flowIntegration.OnRecordSong();

            // Assert
            Assert.AreEqual("Insufficient funds to record a song.", flowIntegration.errorText.text);
            Assert.IsTrue(flowIntegration.errorPanel.activeSelf);
        }

        [Test]
        public void OnGoOnTour_ShouldGoOnTourIfSufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentTourCost;

            // Act
            flowIntegration.OnGoOnTour();

            // Assert
            Assert.AreEqual(flowIntegration.currentMoney - flowIntegration.currentTourCost, flowIntegration.currentMoney);
            Assert.AreEqual(flowIntegration.currentFans + flowIntegration.currentTourCapacity, flowIntegration.currentFans);
            Assert.AreEqual(flowIntegration.currentLevel + 1, flowIntegration.currentLevel);
        }

        [Test]
        public void OnGoOnTour_ShouldShowErrorIfInsufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentTourCost - 1;

            // Act
            flowIntegration.OnGoOnTour();

            // Assert
            Assert.AreEqual("Insufficient funds to go on tour.", flowIntegration.errorText.text);
            Assert.IsTrue(flowIntegration.errorPanel.activeSelf);
        }

        [Test]
        public void OnUpgradeStudio_ShouldUpgradeStudioIfSufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentStudioUpgradeCost;

            // Act
            flowIntegration.OnUpgradeStudio();

            // Assert
            Assert.AreEqual(flowIntegration.currentMoney - flowIntegration.currentStudioUpgradeCost, flowIntegration.currentMoney);
            Assert.AreEqual(flowIntegration.currentStudioLevel + 1, flowIntegration.currentStudioLevel);
            Assert.AreEqual(flowIntegration.currentStudioUpgradeCost * 2, flowIntegration.currentStudioUpgradeCost);
        }

        [Test]
        public void OnUpgradeStudio_ShouldShowErrorIfInsufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentStudioUpgradeCost - 1;

            // Act
            flowIntegration.OnUpgradeStudio();

            // Assert
            Assert.AreEqual("Insufficient funds to upgrade studio.", flowIntegration.errorText.text);
            Assert.IsTrue(flowIntegration.errorPanel.activeSelf);
        }

        [Test]
        public void OnUpgradeTour_ShouldUpgradeTourIfSufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentTourUpgradeCost;

            // Act
            flowIntegration.OnUpgradeTour();

            // Assert
            Assert.AreEqual(flowIntegration.currentMoney - flowIntegration.currentTourUpgradeCost, flowIntegration.currentMoney);
            Assert.AreEqual(flowIntegration.currentTourCapacity * 2, flowIntegration.currentTourCapacity);
            Assert.AreEqual(flowIntegration.currentTourCost * 2, flowIntegration.currentTourCost);
            Assert.AreEqual(flowIntegration.currentTourUpgradeCost * 2, flowIntegration.currentTourUpgradeCost);
        }

        [Test]
        public void OnUpgradeTour_ShouldShowErrorIfInsufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentTourUpgradeCost - 1;

            // Act
            flowIntegration.OnUpgradeTour();

            // Assert
            Assert.AreEqual("Insufficient funds to upgrade tour.", flowIntegration.errorText.text);
            Assert.IsTrue(flowIntegration.errorPanel.activeSelf);
        }

        [Test]
        public void OnUpgradeSong_ShouldUpgradeSongIfSufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentSongUpgradeCost;

            // Act
            flowIntegration.OnUpgradeSong();

            // Assert
            Assert.AreEqual(flowIntegration.currentMoney - flowIntegration.currentSongUpgradeCost, flowIntegration.currentMoney);
            Assert.AreEqual(flowIntegration.currentSongQuality * 2, flowIntegration.currentSongQuality);
            Assert.AreEqual(flowIntegration.currentSongCost * 2, flowIntegration.currentSongCost);
            Assert.AreEqual(flowIntegration.currentSongUpgradeCost * 2, flowIntegration.currentSongUpgradeCost);
        }

        [Test]
        public void OnUpgradeSong_ShouldShowErrorIfInsufficientFunds()
        {
            // Arrange
            flowIntegration.currentMoney = flowIntegration.currentSongUpgradeCost - 1;

            // Act
            flowIntegration.OnUpgradeSong();

            // Assert
            Assert.AreEqual("Insufficient funds to upgrade song quality.", flowIntegration.errorText.text);
            Assert.IsTrue(flowIntegration.errorPanel.activeSelf);
        }

        [Test]
        public void UpdateUI_ShouldUpdateTextElements()
        {
            // Arrange
            flowIntegration.currentMoney = 500;
            flowIntegration.currentFans = 1000;
            flowIntegration.currentLevel = 10;
            flowIntegration.currentStudioLevel = 5;
            flowIntegration.currentSongQuality = 10;
            flowIntegration.currentTourCapacity = 50;
            flowIntegration.currentTourCost = 250;
            flowIntegration.currentSongCost = 100;
            flowIntegration.currentStudioUpgradeCost = 500;
            flowIntegration.currentTourUpgradeCost = 1000;
            flowIntegration.currentSongUpgradeCost = 2000;

            // Act
            flowIntegration.UpdateUI();

            // Assert
            Assert.AreEqual("Money: 500", flowIntegration.moneyText.text);
            Assert.AreEqual("Fans: 1000", flowIntegration.fansText.text);
            Assert.AreEqual("Level: 10", flowIntegration.levelText.text);
            Assert.AreEqual("Studio Level: 5", flowIntegration.studioLevelText.text);
            Assert.AreEqual("Song Quality: 10", flowIntegration.songQualityText.text);
            Assert.AreEqual("Tour Capacity: 50", flowIntegration.tourCapacityText.text);
            Assert.AreEqual("Tour Cost: 250", flowIntegration.tourCostText.text);
            Assert.AreEqual("Song Cost: 100", flowIntegration.songCostText.text);
            Assert.AreEqual("Studio Upgrade Cost: 500", flowIntegration.studioUpgradeCostText.text);
            Assert.AreEqual("Tour Upgrade Cost: 1000", flowIntegration.tourUpgradeCostText.text);
            Assert.AreEqual("Song Upgrade Cost: 2000", flowIntegration.songUpgradeCostText.text);
        }

        [Test]
        public void GeneratePlayerAddress_ShouldGenerateUniqueAddress()
        {
            // Arrange
            string playerId1 = "Player1";
            string playerId2 = "Player2";

            // Act
            string address1 = flowIntegration.GeneratePlayerAddress(playerId1);
            string address2 = flowIntegration.GeneratePlayerAddress(playerId2);

            // Assert
            Assert.AreNotEqual(address1, address2);
        }

        // Mock classes for dependencies
        private class MockFlowClient : FlowClient
        {
            public string SavedPlayerData { get; private set; }
            public string LoadedPlayerData { get; set; }
            public int USDCBalance { get; set; }

            public override void SavePlayerData(string playerId, string jsonData)
            {
                SavedPlayerData = jsonData;
            }

            public override string LoadPlayerData(string playerId)
            {
                return LoadedPlayerData;
            }

            public override System.Threading.Tasks.Task<int> GetUSDCBalance(string usdcContractAddress, string playerAddress)
            {
                return System.Threading.Tasks.Task.FromResult(USDCBalance);
            }

            public override System.Threading.Tasks.Task TransferUSDC(string usdcContractAddress, string playerAddress, int amount)
            {
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

        private class MockBlockchain : Blockchain
        {
            public List<Block> Blocks { get; private set; } = new List<Block>();

            public MockBlockchain(int difficulty = 4) : base(difficulty)
            {
            }

            public override void AddBlock(Block newBlock)
            {
                Blocks.Add(newBlock);
            }
        }
    }
}
