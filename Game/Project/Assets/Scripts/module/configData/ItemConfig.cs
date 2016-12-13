using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xk_System.Debug;

namespace xk_System.Db
{
    public class ItemData
    {
        public int MaxLayer;
        public ItemDB mItemDB;
        public List<SubItem> mSubItemList=new List<SubItem>();
    }

    public class SubItem
    {
        /// <summary>
        /// 表示第几层
        /// </summary>
        public int Layer;
        /// <summary>
        /// 表示子2茶树的索引（从左到右）
        /// </summary>
        public int index;
        /// <summary>
        /// 我兄弟的个数
        /// </summary>
        public int cout;
        /// <summary>
        /// 父节点
        /// </summary>
        public SubItem mItemDBParent;
        /// <summary>
        /// 我
        /// </summary>
        public ItemDB mItemDB;   
    }

    public class ItemConfig : Singleton<ItemConfig>
    {
        public List<ItemDB> mItemConfig = null;
        public ItemConfig()
        {
            mItemConfig = DbManager.Instance.GetDb<ItemDB>();
        }

        public ItemDB FindItem(int id)
        {
            return mItemConfig.Find((x) => x.id == id);
        }

        public List<ItemDB> GetCanCompoundItemList(int id)
        {
            List<ItemDB> mlist = new List<ItemDB>();
            foreach (var v in mItemConfig)
            {
                if (v.SubItemArray.Contains(id))
                {
                    mlist.Add(v);
                }
            }
            return mlist;
        }

        public List<ItemData> GetItemDataList(List<int> mItemIdList)
        {
            List<ItemData> mItemDataList = new List<ItemData>();
            foreach (var v in mItemIdList)
            {
                mItemDataList.Add(GetItemData(v));
            }
            return mItemDataList;
        }

        public ItemData GetItemData(int id)
        {
            ItemData mItemData = new ItemData();
            ItemDB mItemDB = mItemConfig.Find((x) => x.id == id);
            mItemData.mItemDB = mItemDB;

            SubItem mSubItem = new SubItem();
            mSubItem.Layer = 0;
            mSubItem.index = 0;
            mSubItem.mItemDBParent =null;
            mSubItem.cout = 1;
            mSubItem.mItemDB = mItemDB;
            mItemData.mSubItemList.Add(mSubItem);

            if (mItemDB.SubItemArray!=null && mItemDB.SubItemArray.Count > 0)
            {
                GetSubItemList(mSubItem, 0, mItemData);
            }
            return mItemData;
        }

        private void GetSubItemList(SubItem mSubItemDBParent, int Layer, ItemData mItemData)
        {
            Layer++;
            if (Layer + 1 > mItemData.MaxLayer)
            {
                mItemData.MaxLayer=Layer+1;
            }
            int cout = mSubItemDBParent.mItemDB.SubItemArray.Count;
            for (int i = 0; i < mSubItemDBParent.mItemDB.SubItemArray.Count; i++)
            {
                ItemDB mItemDB1 = mItemConfig.Find((x) => x.id == mSubItemDBParent.mItemDB.SubItemArray[i]);
                if (mItemDB1 == null)
                {
                    DebugSystem.LogError("物品找不到：" + mSubItemDBParent.mItemDB.SubItemArray[i]);
                    continue;
                }
                SubItem mSubItem = new SubItem();
                mSubItem.Layer = Layer;
                mSubItem.index = i;
                mSubItem.mItemDBParent = mSubItemDBParent;
                mSubItem.cout = cout;
                mSubItem.mItemDB = mItemDB1;
                mItemData.mSubItemList.Add(mSubItem);

                if (mItemDB1.SubItemArray != null && mItemDB1.SubItemArray.Count > 0)
                {
                    GetSubItemList(mSubItem, Layer, mItemData);
                }
            }
        }
    }
   
}