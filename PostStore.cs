/*
	This file is part of NKK.

	NKK is free software: you can redistribute it and/or modify it under the
	terms of the GNU Affero General Public License as published by the Free
	Software Foundation, either version 3 of the License, or (at your option)
	any later version.

	NKK is distributed in the hope that it will be useful, but WITHOUT ANY
	WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
	FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for
	more details.

	You should have received a copy of the GNU Affero General Public License
	along with NKK. If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Concurrent;

namespace NKK;

public class PostStore {
	private readonly ConcurrentDictionary<Type, ConcurrentDictionary<String, Object>> _posts = new();
	
	public IEnumerable<T> GetAll<T>() where T : class, IPostPayload {
		if (!this._posts.ContainsKey(typeof(T)))
			return [];
		
		return this._posts[typeof(T)].Values.Cast<T>();
	}

	public T? Get<T>(String id) where T : class, IPostPayload {
		return this._posts[typeof(T)].TryGetValue(id, out var post) ? (T)post : null;
	}

	public void AddOrUpdate<T>(String id, T post) where T : class, IPostPayload {
		if (!this._posts.ContainsKey(typeof(T)))
			this._posts[typeof(T)] = new ConcurrentDictionary<String, Object>();
		
		this._posts[typeof(T)][id] = post;
	}

	public void Remove<T>(String id) where T : class, IPostPayload {
		this._posts[typeof(T)].Remove(id, out _);
	}
}