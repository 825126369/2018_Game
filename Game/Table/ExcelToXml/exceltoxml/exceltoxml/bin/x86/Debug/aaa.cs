namespace Db
{
	class Sheet1
	{
		/// <summary>
		/// 字段1
		/// </summary>
		public readonly int aaa
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly int ccc

		public Sheet1(int aaa,string bbb,int ccc)
		{
			this.aaa=aaa;
			this.bbb=bbb;
			this.ccc=ccc;
		}
		public Sheet1(ArrayList list)
		{
			FieldInfo[] mFieldInfo = this.GetType().GetFields();
			for (int i = 0; i < mFieldInfo.Length; i++)
			{
				mFieldInfo[i].SetValue(this, list[i]);
			}
		}
	}

}