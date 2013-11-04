using System.Collections.Generic;
using nobnak.Collection;

namespace nobnak.Geometry {
	public class FaceDatabase {
		private Dictionary<int, LinkedList<Face>> _vertex2face;
		private HashSet<Face> _faces;
		
		public FaceDatabase() {
			this._vertex2face = new Dictionary<int, LinkedList<Face>>();
			this._faces = new HashSet<Face>();
		}
		
		public void Add(Face f) {
			_faces.Add(f);
			foreach (var v in f) {
				var faces = _vertex2face.ContainsKey(v) ? _vertex2face[v] : _vertex2face[v] = new LinkedList<Face>();
				if (!faces.Contains(f))
					faces.AddLast(f);
			}
		}
		public void Remove(Face f) {
			_faces.Remove(f);
			foreach (var v in f) {
				if (!_vertex2face.ContainsKey(v))
					continue;
				var faces = _vertex2face[v];
				faces.Remove(f);
				if (faces.Count == 0)
					_vertex2face.Remove(v);
			}
		}
		
		public Face[] GetAdjacentFaces(int v) {
			if (!_vertex2face.ContainsKey(v))
				return new Face[0];
			var faces = _vertex2face[v];
			var res = new Face[faces.Count];
			faces.CopyTo(res, 0);
			return res;
		}
		public Face[] GetAdjacentFaces(Edge e) {
			return GetIntersection(new Face[][] { GetAdjacentFaces(e.v0), GetAdjacentFaces(e.v1) });
		}
		public Edge[] GetAdjacentEdges(int v) {
			var founds = new HashSet<Edge>();
			foreach (var f in GetAdjacentFaces(v)) {
				for (var i = 0; i < 3; i++) {
					var edge = new Edge(f[i], f[i+1]);
					if (!edge.Contains(v) || founds.Contains(edge))
						continue;
					founds.Add(edge);
				}
			}
			var res = new Edge[founds.Count];
			founds.CopyTo(res, 0);
			return res;
		}
		
		public Face[] GetIntersection(IEnumerable<Face>[] faceSets) {
			var counter = new HashCounter<Face>();
			foreach (var faces in faceSets) {
				foreach (var f in faces) {
					counter[f]++;
				}
			}
			var res = new List<Face>();
			foreach (var f in counter) {
				if (counter[f] == faceSets.Length)
					res.Add(f);
			}
			return res.ToArray();
		}
		public Face[] GetUnion(IEnumerable<Face>[] faceSets) {
			var founds = new HashSet<Face>();
			foreach (var fset in faceSets) {
				foreach (var f in fset) {
					if (founds.Contains(f))
						continue;
					founds.Add(f);
				}
			}
			var res = new Face[founds.Count];
			founds.CopyTo(res, 0);
			return res;
		}

		public IEnumerable<Face> Faces { 
			get { 
				var res = new Face[_faces.Count];
				_faces.CopyTo(res, 0);
				return res;
			}
		}
		public IEnumerable<int> Vertices { 
			get { 
				var res = new int[_vertex2face.Keys.Count]; 
				_vertex2face.Keys.CopyTo(res, 0);
				return res;
			} 
		}
	}

	public class Face : IEnumerable<int> {
		private FaceDatabase _db;
		private int _v0, _v1, _v2;
		
		public int v0 {
			get { return _v0; }
			set {
				_db.Remove(this);
				_v0 = value;
				_db.Add(this);
			}
		}
		public int v1 {
			get { return _v1; }
			set {
				_db.Remove(this);
				_v1 = value;
				_db.Add(this);
			}
		}
		public int v2 {
			get { return _v2; }
			set { 
				_db.Remove(this);
				_v2 = value;
				_db.Add(this);
			}
		}
		
		public Face(FaceDatabase db, int v0, int v1, int v2) {
			this._db = db;
			this._v0 = v0;
			this._v1 = v1;
			this._v2 = v2;
			this._db.Add(this);			
		}
		
		public int this[int i] {
			get {
				i %= 3;
				return i == 0 ? v0 : (i == 1 ? v1 : v2);
			}
		}
		
		public bool Contains(int v) {
			return v0 == v || v1 == v || v2 == v;
		}
		
		public void Renumber(int v0, int v1) {
			_db.Remove(this);
			if (_v0 == v0)
				_v0 = v1;
			if (_v1 == v0)
				_v1 = v1;
			if (_v2 == v0)
				_v2 = v1;
			_db.Add(this);
		}
		
		public override string ToString () {
			return string.Format("Face({0},{1},{2})", v0, v1, v2);
		}
		
		public override int GetHashCode () {
			return 83 * (v0 + 151 * (v1 + 19 * v2));
		}
		public override bool Equals (object obj) {
			var f = obj as Face;
			return f != null && f.v0 == v0 && f.v1 == v1 && f.v2 == v2;
		}

		#region IEnumerable[System.Int32] implementation
		public IEnumerator<int> GetEnumerator () {
			yield return v0;
			yield return v1;
			yield return v2;
		}
		#endregion
		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () 	{
			yield return v0;
			yield return v1;
			yield return v2;
		}
		#endregion
	}
	public class Edge : IEnumerable<int> {
		public int v0, v1;
		
		public Edge(int v0, int v1) {
			if (v0 < v1) {
				this.v0 = v0;
				this.v1 = v1;
			} else {
				this.v0 = v1;
				this.v1 = v0;
			}
		}
		
		public int this[int i] {
			get {
				i %= 2;
				return i == 0 ? v0 : v1;
			}
		}
		
		public bool Contains(int v) {
			return v0 == v || v1 == v;
		}
		
		public override int GetHashCode () {
			return 71 * (v0 + 19 * v1);
		}
		public override bool Equals (object obj) {
			var e = obj as Edge;
			return (e != null && e.v0 == v0 && e.v1 == v1);
		}
		
		public override string ToString () {
			return string.Format("Edge({0},{1})", v0, v1);
		}

		#region IEnumerable[System.Int32] implementation
		public IEnumerator<int> GetEnumerator () {
			yield return v0;
			yield return v1;
		}
		#endregion

		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
			yield return v0;
			yield return v1;
		}
		#endregion
	}
	
	
}