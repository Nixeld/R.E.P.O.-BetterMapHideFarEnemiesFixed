using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HideEnemiesMap
{
	[HarmonyPatch(typeof(EnemyParent), "SpawnRPC")]
	public static class EnemySpawnRPCHookPatch
	{
		private static void Postfix(EnemyParent __instance)
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Expected I4, but got Unknown
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			if (!HideEnemiesMap.configShowEnemies.Value)
			{
				return;
			}
			FieldInfo field = typeof(EnemyParent).GetField("Enemy", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object? value = field.GetValue(__instance);
			Enemy val = (Enemy)((value is Enemy) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			GameObject mapCustomGameObject = MapUtils.GetMapCustomGameObject(val);
			MapCustom component = mapCustomGameObject.GetComponent<MapCustom>();
			if ((Object)(object)component == (Object)null)
			{
				component = mapCustomGameObject.AddComponent<MapCustom>();
				switch ((int)__instance.difficulty)
				{
				case 0:
					component.sprite = HideEnemiesMap.enemySpriteSquare;
					component.color = Color.red;
					break;
				case 1:
					component.sprite = HideEnemiesMap.enemySpriteCircle;
					component.color = Color.red;
					break;
				case 2:
					component.sprite = HideEnemiesMap.enemySpriteTriangle;
					component.color = Color.red;
					break;
				default:
					component.sprite = HideEnemiesMap.enemySpriteSquare;
					component.color = Color.red;
					break;
				}
			}
		}
	}
	[HarmonyPatch(typeof(EnemyParent), "DespawnRPC")]
	public static class EnemyDeSpawnRPCHookPatch
	{
		private static void Postfix(EnemyParent __instance)
		{
			if (!HideEnemiesMap.configShowEnemies.Value)
			{
				return;
			}
			FieldInfo field = typeof(EnemyParent).GetField("Enemy", BindingFlags.Instance | BindingFlags.NonPublic);
			if (!(field != null))
			{
				return;
			}
			object? value = field.GetValue(__instance);
			Enemy val = (Enemy)((value is Enemy) ? value : null);
			if (!((Object)(object)val != (Object)null))
			{
				return;
			}
			MapCustom component = MapUtils.GetMapCustomGameObject(val).GetComponent<MapCustom>();
			if ((Object)(object)component != (Object)null)
			{
				if ((Object)(object)component.mapCustomEntity != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)component.mapCustomEntity).gameObject);
				}
				Object.Destroy((Object)(object)component);
			}
		}
	}
	[HarmonyPatch(typeof(LevelGenerator), "GenerateDone")]
	public static class LevelGeneratorGenerateDoneHookPatch
	{
		private static void Postfix()
		{
			MapUtils.AddAllPlayersToMap();
			if (!HideEnemiesMap.configExploreAllRooms.Value)
			{
				return;
			}
			RoomVolume[] array = Object.FindObjectsOfType<RoomVolume>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetExplored();
			}
		}
	}
	[HarmonyPatch(typeof(PlayerAvatar))]
	public static class PlayerAvatarRPCPatch
	{
		[HarmonyPatch("ReviveRPC")]
		[HarmonyPostfix]
		public static void ReviveRPC(PlayerAvatar __instance)
		{
			MapUtils.AddPlayerToMap(__instance);
		}

		[HarmonyPatch("PlayerDeathRPC")]
		[HarmonyPostfix]
		public static void PlayerDeathRPC(PlayerAvatar __instance)
		{
			MapUtils.RemovePlayerFromMap(__instance);
		}
	}
	[HarmonyPatch(typeof(EnemyParent), "GetRoomVolume")]
	public class Patch_EnemyParent_GetRoomVolume
	{
		private static FieldInfo PlayerFieldInfo = AccessTools.Field(typeof(RoomVolumeCheck), "player");

		private static FieldInfo CurrentRoomsField = AccessTools.Field(typeof(EnemyParent), "currentRooms");

		private static FieldInfo EnemyField = typeof(EnemyParent).GetField("Enemy", BindingFlags.Instance | BindingFlags.NonPublic);

		private static float _lastUpdateTime = -1f;

		private static float _updateCooldown = 1f;

		private static HashSet<int> _cachedPlayerRoomHashes = new HashSet<int>();

		private static Dictionary<MapCustom, Coroutine> activeCoroutines = new Dictionary<MapCustom, Coroutine>();

		private static void Postfix(EnemyParent __instance)
		{
			if (!HideEnemiesMap.configHideFarEnemies.Value)
			{
				return;
			}
			if (Time.unscaledTime - _lastUpdateTime >= _updateCooldown)
			{
				_lastUpdateTime = Time.unscaledTime;
				UpdatePlayerRoomHashes();
			}
			if (!(CurrentRoomsField.GetValue(__instance) is List<RoomVolume> list))
			{
				return;
			}
			object? obj = EnemyField?.GetValue(__instance);
			Enemy val = (Enemy)((obj is Enemy) ? obj : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			GameObject mapCustomGameObject = MapUtils.GetMapCustomGameObject(val);
			MapCustom val2 = ((mapCustomGameObject != null) ? mapCustomGameObject.GetComponent<MapCustom>() : null);
			if ((Object)(object)val2 == (Object)null || (Object)(object)val2?.mapCustomEntity?.spriteRenderer == (Object)null)
			{
				return;
			}
			bool state = false;
			foreach (RoomVolume item in list)
			{
				if (_cachedPlayerRoomHashes.Contains(((object)item).GetHashCode()))
				{
					state = true;
					break;
				}
			}
			ChangeVisibility(val2, state);
		}

		private static void ChangeVisibility(MapCustom component, bool state)
		{
			if ((Object)(object)component.mapCustomEntity?.spriteRenderer != (Object)null)
			{
				if (((Renderer)component.mapCustomEntity.spriteRenderer).enabled != state)
				{
					((Renderer)component.mapCustomEntity.spriteRenderer).enabled = state;
				}
			}
			else
			{
				Debug.Log((object)"[Better Map Hide Far Enemies Fixed] spriteRenderer is null.");
			}
		}

		private static void UpdatePlayerRoomHashes()
		{
			_cachedPlayerRoomHashes.Clear();
			RoomVolumeCheck[] array = Object.FindObjectsOfType<RoomVolumeCheck>();
			RoomVolumeCheck[] array2 = array;
			foreach (RoomVolumeCheck val in array2)
			{
				if (!(bool)PlayerFieldInfo.GetValue(val))
				{
					continue;
				}
				foreach (RoomVolume currentRoom in val.CurrentRooms)
				{
					_cachedPlayerRoomHashes.Add(((object)currentRoom).GetHashCode());
				}
			}
		}
	}
	[HarmonyPatch]
	internal static class Patches
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(ValuableObject), "Start")]
		private static void Postfix(ValuableObject __instance)
		{
			if (HideEnemiesMap.configExploreValuables.Value)
			{
				Map.Instance.AddValuable(__instance);
			}
		}
	}
	[BepInPlugin("Better.Map.Hide.Far.Enemies.Fixed", "Better Map Hide Far Enemies Fixed", "1.0.0")]
	public class HideEnemiesMap : BaseUnityPlugin
	{
		internal static ConfigEntry<bool> configShowTeammates;

		internal static ConfigEntry<bool> configShowEnemies;

		internal static ConfigEntry<bool> configHideFarEnemies;

		internal static ConfigEntry<bool> configExploreAllRooms;

		internal static ConfigEntry<bool> configExploreValuables;

		internal static Sprite enemySpriteSquare;

		internal static Sprite enemySpriteCircle;

		internal static Sprite enemySpriteTriangle;

		private void Awake()
		{
			Logger.LogInfo("Better Map Hide Far Enemies Fixed");
			configShowTeammates = Config.Bind("Display Options", "ShowTeammates", true, "Display teammate on the map");
			configShowEnemies = Config.Bind("Display Options", "ShowEnemies", true, "Display enemy on the map");
			configHideFarEnemies = Config.Bind("Display Options", "HideFarEnemies", true, "Hide enemy unless share room with any player");
			configExploreAllRooms = Config.Bind("Gameplay Options", "ExploreAllRooms", false, "Automatically explore all rooms at the start of the game");
			configExploreValuables = Config.Bind("Gameplay Options", "ExploreValuables", false, "All valuables are visible on the map");
			new Harmony("Better.Map.Hide.Far.Enemies.Fixed").PatchAll(Assembly.GetExecutingAssembly());
			MapUtils.CreateEnemySprites();
		}
	}
	public static class MapUtils
	{
		private static FieldInfo field = typeof(Enemy).GetField("Rigidbody", BindingFlags.Instance | BindingFlags.NonPublic);

		private static FieldInfo field2 = typeof(Enemy).GetField("HasRigidbody", BindingFlags.Instance | BindingFlags.NonPublic);

		public static void CreateEnemySprites()
		{
			if (!((Object)(object)HideEnemiesMap.enemySpriteSquare != (Object)null) || !((Object)(object)HideEnemiesMap.enemySpriteCircle != (Object)null) || !((Object)(object)HideEnemiesMap.enemySpriteTriangle != (Object)null))
			{
				HideEnemiesMap.enemySpriteSquare = CreateSquareSprite();
				HideEnemiesMap.enemySpriteCircle = CreateCircleSprite();
				HideEnemiesMap.enemySpriteTriangle = CreateTriangleSprite();
			}
		}

		public static GameObject GetMapCustomGameObject(Enemy enemy)
		{
			if ((Object)(object)enemy == (Object)null)
			{
				return null;
			}
			EnemyRigidbody item = GetEnemyRigidbody(enemy).Item1;
			if (!((Object)(object)item != (Object)null))
			{
				return ((Component)enemy).gameObject;
			}
			return ((Component)item).gameObject;
		}

		public static (EnemyRigidbody, bool) GetEnemyRigidbody(Enemy enemy)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			if (field == null || field2 == null)
			{
				return (null, false);
			}
			EnemyRigidbody item = (EnemyRigidbody)field.GetValue(enemy);
			bool item2 = (bool)field2.GetValue(enemy);
			return (item, item2);
		}

		public static void AddPlayerToMap(PlayerAvatar playerAvatar)
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if (HideEnemiesMap.configShowTeammates.Value && !((Object)(object)playerAvatar == (Object)null) && !((Object)(object)((Component)playerAvatar).GetComponent<MapCustom>() != (Object)null))
			{
				MapCustom val = ComponentHolderProtocol.AddComponent<MapCustom>((Object)(object)playerAvatar);
				FieldInfo fieldInfo = typeof(PlayerAvatarVisuals).GetField("color", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					Color color = (Color)fieldInfo.GetValue(playerAvatar.playerAvatarVisuals);
					val.sprite = HideEnemiesMap.enemySpriteSquare;
					val.color = color;
				}
			}
		}

		public static void RemovePlayerFromMap(PlayerAvatar playerAvatar)
		{
			if (!HideEnemiesMap.configShowTeammates.Value || (Object)(object)playerAvatar == (Object)null)
			{
				return;
			}
			MapCustom component = ((Component)playerAvatar).GetComponent<MapCustom>();
			if (!((Object)(object)component == (Object)null))
			{
				if ((Object)(object)component.mapCustomEntity != (Object)null)
				{
					Object.Destroy((Object)(object)component.mapCustomEntity);
				}
				Object.Destroy((Object)(object)component);
			}
		}

		public static void AddAllPlayersToMap()
		{
			if (!HideEnemiesMap.configShowTeammates.Value || (Object)(object)GameDirector.instance == (Object)null)
			{
				return;
			}
			List<PlayerAvatar> playerList = GameDirector.instance.PlayerList;
			if (playerList == null || playerList.Count == 0)
			{
				return;
			}
			foreach (PlayerAvatar item in playerList)
			{
				AddPlayerToMap(item);
			}
		}

		private static Sprite CreateSquareSprite()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			int num = 5;
			Texture2D val = new Texture2D(num, num, (TextureFormat)5, false);
			Color[] array = (Color[])(object)new Color[num * num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Color.white;
			}
			val.SetPixels(array);
			val.Apply();
			return Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num), new Vector2(0.5f, 0.5f));
		}

		private static Sprite CreateCircleSprite()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			int num = 10;
			Texture2D val = new Texture2D(num, num, (TextureFormat)5, false);
			Color[] array = (Color[])(object)new Color[num * num];
			float num2 = (float)num / 2f;
			float num3 = (float)num / 2f - 1f;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					int num4 = i * num + j;
					float num5 = (float)j - num2;
					float num6 = (float)i - num2;
					if (num5 * num5 + num6 * num6 <= num3 * num3)
					{
						array[num4] = Color.white;
					}
					else
					{
						array[num4] = Color.clear;
					}
				}
			}
			val.SetPixels(array);
			val.Apply();
			return Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num), new Vector2(0.5f, 0.5f));
		}

		private static Sprite CreateTriangleSprite()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			int num = 10;
			Texture2D val = new Texture2D(num, num, (TextureFormat)5, false);
			Color[] array = (Color[])(object)new Color[num * num];
			Vector2 a = new Vector2((float)num / 2f, (float)(num - 1));
			Vector2 b = new Vector2(1f, 1f);
			Vector2 c = new Vector2((float)(num - 2), 1f);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					Vector2 p = new Vector2((float)j, (float)i);
					int num2 = i * num + j;
					if (IsPointInTriangle(p, a, b, c))
					{
						array[num2] = Color.white;
					}
					else
					{
						array[num2] = Color.clear;
					}
				}
			}
			val.SetPixels(array);
			val.Apply();
			return Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num), new Vector2(0.5f, 0.5f));
		}

		private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			float num = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
			float num2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
			float num3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
			bool flag = num < 0f || num2 < 0f || num3 < 0f;
			bool flag2 = num > 0f || num2 > 0f || num3 > 0f;
			return !flag || !flag2;
		}
	}
}