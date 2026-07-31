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
	// dict by Type (T : IPostPayload) to another dict
	// *that* dict is of string (post fullId's) to instances of the aforementioned T
	private readonly ConcurrentDictionary<Type, ConcurrentDictionary<String, Object>> _posts = new();
	
	/// <summary>
	///		Get all posts of a certain type
	/// </summary>
	/// <typeparam name="T">
	///		Implementor of <see cref="IPostPayload"/>
	///	</typeparam>
	/// <returns>
	///		<see cref="IEnumerable"/> of posts
	/// </returns>
	public IEnumerable<T> GetAll<T>() where T : class, IPostPayload {
		if (!this._posts.ContainsKey(typeof(T)))
			return [];
		
		return this._posts[typeof(T)].Values.Cast<T>();
	}
	
	/// <summary>
	///		Try to get a post by its <see cref="PostId"/>
	/// </summary>
	/// <param name="id">
	///		Full match for some post's <see cref="PostId"/>
	/// </param>
	/// <typeparam name="T">
	///		Implementor of <see cref="IPostPayload"/>
	/// </typeparam>
	/// <returns>
	///		Corresponding post if found, or null
	/// </returns>
	public T? Get<T>(PostId id) where T : class, IPostPayload {
		if (!this._posts.ContainsKey(typeof(T)))
			return null;
		
		if (!this._posts[typeof(T)].TryGetValue(id.FullId, out var post))
			return null;
		
		return (T)post;
	}
	
	/// <summary>
	///		Add (or update) a post in the store by <see cref="PostId.FullId"/>
	/// </summary>
	/// <param name="id">
	///		ID bundle. Updates only on full match, otherwise adds
	/// </param>
	/// <param name="post">
	///		IPostPayload to add/update
	/// </param>
	/// <typeparam name="T">
	///		Implementor of <see cref="IPostPayload"/>
	/// </typeparam>
	public void AddOrUpdate<T>(PostId id, T post) where T : class, IPostPayload {
		if (!this._posts.ContainsKey(typeof(T)))
			this._posts[typeof(T)] = new ConcurrentDictionary<String, Object>();
		
		this._posts[typeof(T)][id.FullId] = post;
	}
	
	/// <summary>
	///		Remove a post from the store by <see cref="PostId"/>.
	/// </summary>
	/// <param name="id">
	///		Full match for some post's <see cref="PostId"/>. Will accept anything,
	///		this method does not error if there is no such post by that ID.
	/// </param>
	/// <typeparam name="T">
	///		Implementor of <see cref="IPostPayload"/>
	/// </typeparam>
	public void Remove<T>(PostId id) where T : class, IPostPayload {
		if (this._posts.ContainsKey(typeof(T)))
			this._posts[typeof(T)].Remove(id.FullId, out _);
	}
}