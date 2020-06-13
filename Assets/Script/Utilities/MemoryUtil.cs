using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 内存废弃处理类
/// </summary>
public static class MemoryUtil {
	/// <summary>
	/// List废弃处理
	/// </summary>
	public static void SetNull<T>(ref List<T> list) where T : class {
		if (list == null) {
			return;
		}
		for (int i = 0; i < list.Count; i++) {
			if (list[i] != null && list[i] is IDestroyable) {
				((IDestroyable)list[i]).Destroy();
			}
			list[i] = null;
		}
		list = null;
	}

	/// <summary>
	/// Array废弃处理
	/// </summary>
	public static void SetNull<T>(ref T[] array) where T : class {
		if (array == null) {
			return;
		}
		for (int i = 0; i < array.Length; ++i) {
			if (array[i] != null && array[i] is IDestroyable) {
				((IDestroyable)array[i]).Destroy();
			}
			array[i] = null;
		}
		array = null;
	}
}

/// <summary>
/// 拥有废弃处理的Interface
/// </summary>
public interface IDestroyable {
	void Destroy();
}
