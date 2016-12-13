using System.Collections;
using System.Collections.Generic;
namespace xk_System.Db
{
	public class HeroAttDB:DbBase
	{
		/// <summary>
		/// 英雄ID
		/// </summary>
		public readonly int heroId;
		/// <summary>
		/// 英雄名
		/// </summary>
		public readonly string heroName;
		/// <summary>
		/// 英雄Atlas
		/// </summary>
		public readonly string heroAtlas;
		/// <summary>
		/// 英雄Icon数组
		/// </summary>
		public readonly string heroIcon;
		/// <summary>
		/// 英雄模型Bundle
		/// </summary>
		public readonly string heroModelBundle;
		/// <summary>
		/// 英雄模型名
		/// </summary>
		public readonly string HeroModelName;
		/// <summary>
		/// 基础生命值
		/// </summary>
		public readonly int Hp;
		/// <summary>
		/// 等级加成生命值千分比
		/// </summary>
		public readonly int Level;
		/// <summary>
		/// 法力资源类型（0：不消耗1：法力值2：能量值3：怒气）
		/// </summary>
		public readonly int MPType;
		/// <summary>
		/// 基础法力值
		/// </summary>
		public readonly int MPValue;
		/// <summary>
		/// 等级加成法力值百分比
		/// </summary>
		public readonly int MP;
		/// <summary>
		/// 基础攻击力
		/// </summary>
		public readonly int bbb3;
		/// <summary>
		/// 等级加成攻击力百分比
		/// </summary>
		public readonly int LevelAddAttackPercent;
		/// <summary>
		/// 基础法强
		/// </summary>
		public readonly int bbb4;
		/// <summary>
		/// 等级加成法强百分比
		/// </summary>
		public readonly int LevelAddMAttackPercent;
		/// <summary>
		/// 基础护甲
		/// </summary>
		public readonly int bbb5;
		/// <summary>
		/// 等级加成护甲百分比
		/// </summary>
		public readonly int LevelAddDefPercent;
		/// <summary>
		/// 等级加成魔抗百分比
		/// </summary>
		public readonly int LevelAddMDefPercent;
		/// <summary>
		/// 基础移动速度
		/// </summary>
		public readonly int MoveSpeed;
		/// <summary>
		/// 等级加成移动速度
		/// </summary>
		public readonly int LevelAddMoveSpeed;
		/// <summary>
		/// 基础回复生命值速度
		/// </summary>
		public readonly int HPSpeed;
		/// <summary>
		/// 等级加成基础生命值回复速度
		/// </summary>
		public readonly int LevelAddHpSpeedPercent;
		/// <summary>
		/// 基础法力资源回复速度
		/// </summary>
		public readonly int qweqe;
		/// <summary>
		/// 等级加成法力资源回复速度
		/// </summary>
		public readonly int t;
	}

	public class ItemDB:DbBase
	{
		/// <summary>
		/// 物品类型
		/// </summary>
		public readonly List<int> TypeArray=new List<int>();
		/// <summary>
		/// 物品名称
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// 子物品
		/// </summary>
		public readonly List<int> SubItemArray=new List<int>();
		/// <summary>
		/// 合成价格
		/// </summary>
		public readonly int CompoundPrice;
		/// <summary>
		/// 物品购买总价格
		/// </summary>
		public readonly int BuyPrice;
		/// <summary>
		/// 物品出售价格
		/// </summary>
		public readonly int SellPrice;
		/// <summary>
		/// 物品简单描述
		/// </summary>
		public readonly string simpleDes;
		/// <summary>
		/// 物品属性描述
		/// </summary>
		public readonly string AttDes;
	}

	public class ItemAttDB:DbBase
	{
		/// <summary>
		/// 生命值
		/// </summary>
		public readonly int hp;
		/// <summary>
		/// 法力值
		/// </summary>
		public readonly int mp;
		/// <summary>
		/// 护甲
		/// </summary>
		public readonly int def;
		/// <summary>
		/// 暴击几率百分比
		/// </summary>
		public readonly int CritRatePercent;
		/// <summary>
		/// 暴击伤害百分比
		/// </summary>
		public readonly int CritDamagePercent;
		/// <summary>
		/// 生命值回复速度
		/// </summary>
		public readonly int hpRegen;
		/// <summary>
		/// 法力资源回复速度
		/// </summary>
		public readonly int mpRegen;
		/// <summary>
		/// 移动速度
		/// </summary>
		public readonly int moveSpeed;
		/// <summary>
		/// 韧性
		/// </summary>
		public readonly int tough;
		/// <summary>
		/// 护甲穿透
		/// </summary>
		public readonly int ArmorPenetration;
		/// <summary>
		/// 法术穿透
		/// </summary>
		public readonly int SpellPenetration;
	}

	public class ServerListDB:DbBase
	{
		/// <summary>
		/// 区ID
		/// </summary>
		public readonly int serverId;
		/// <summary>
		/// 区名
		/// </summary>
		public readonly string serverName;
	}

	public class skillDB:DbBase
	{
		/// <summary>
		/// 技能ID
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 技能名字
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 技能描述
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 技能Atlas
		/// </summary>
		public readonly string aaa;
		/// <summary>
		/// 技能Icon
		/// </summary>
		public readonly string bbb;
	}

	public class Sheet1DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

	public class Sheet2DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

	public class Sheet3DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

	public class Sheet4DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

}