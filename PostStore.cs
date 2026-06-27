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
	private readonly ConcurrentDictionary<String, SayingPayload> _sayings = new();
	private readonly ConcurrentDictionary<String, ArtifactPayload> _artifacts = new();

	#region Sayings
	public IEnumerable<SayingPayload> GetAllSayings() {
		return this._sayings.Values;
	}
	
	public SayingPayload GetSaying(String id) {
		return this._sayings[id];
	}

	public void AddOrUpdateSaying(String id, SayingPayload saying) {
		this._sayings[id] = saying;
	}

	public void RemoveSaying(String id) {
		this._sayings.Remove(id, out _);
	}
	#endregion
	
	#region Artifacts
	public IEnumerable<ArtifactPayload> GetAllArtifacts() {
		return this._artifacts.Values.OrderByDescending(p => p.Id);
	}
	
	public ArtifactPayload GetArtifact(String id) {
		return this._artifacts[id];
	}

	public void AddOrUpdateArtifact(String id, ArtifactPayload saying) {
		this._artifacts[id] = saying;
	}

	public void RemoveArtifact(String id) {
		this._artifacts.Remove(id, out _);
	}
	#endregion
}