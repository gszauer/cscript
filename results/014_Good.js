class vec2 {
	constructor(x, y) {
		this.x = (typeof x === "undefined")? 0.0 : x;
		this.y = (typeof y === "undefined")? 0.0 : y;
	}
	static epsilon = 0.0001;

	static add = function (left, right) {
		return new vec2(left.x + right.x, left.y + right.y);
	}

	static sub = function (left, right) {
		return new vec2(left.x - right.x, left.y - right.y);
	}

	static mul = function (left, right) {
		return new vec2(left.x * right.x, left.y * right.y);
	}

	static div = function (left, right) {
		return new vec2(left.x / right.x, left.y / right.y);
	}

	static scale = function (left, right) {
		return new vec2(left.x * right, left.y * right);
	}

	static dot = function (left, right) {
		return left.x * right.x + left.y * right.y;
	}

	static len = function (v) {
		let res = v.x * v.x + v.y * v.y;
		if (res < vec2.epsilon) {
			return 0.0;
		}
		return Math.sqrt(res);
	}

	static lenSq = function (v) {
		return v.x * v.x + v.y * v.y;
	}

	static normalized = function (v) {
		let res = v.x * v.x + v.y * v.y;
		if (res < vec2.epsilon) {
			return v;
		}
		let invLen = 1.0 / Math.sqrt(res);
		return new vec2(v.x * invLen, v.y * invLen);
	}

	static angle = function (l, r) {
		let sqMagL = l.x * l.x + l.y * l.y;
		let sqMagR = r.x * r.x + r.y * r.y;
		if (sqMagL < vec2.epsilon || sqMagR < vec2.epsilon) {
			return 0.0;
		}
		let dot = l.x * r.x + l.y * r.y;
		let len = Math.sqrt(sqMagL) * Math.sqrt(sqMagR);
		return Math.acos(dot / len);
	}

	static project = function (a, b) {
		let magBSq = _vec2_len(b);
		if (magBSq < vec2.epsilon) {
			return new vec2();
		}
		let scale = _vec2_dot(a, b) / magBSq;
		return b * scale;
	}

	static reject = function (a, b) {
		let projection = _vec2_project(a, b);
		return vec2.sub(a, projection);
	}

	static reflect = function (a, b) {
		let magBSq = _vec2_len(b);
		if (magBSq < vec2.epsilon) {
			return new vec2();
		}
		let scale = _vec2_dot(a, b) / magBSq;
		let proj2 = b * (scale * 2);
		return vec2.sub(a, proj2);
	}

	static lerp = function (s, e, t) {
		return new vec2(s.x + (e.x - s.x) * t, s.y + (e.y - s.y) * t);
	}

	static slerp = function (s, e, t) {
		if (t < vec2.epsilon) {
			return _vec2_lerp(s, e, t);
		}
		let from = _vec2_normalized(s);
		let to = _vec2_normalized(e);
		let theta = _vec2_angle(from, to);
		let sin_theta = Math.sin(theta);
		let a = Math.sin((1.0 - t) * theta) / sin_theta;
		let b = Math.sin(t * theta) / sin_theta;
		return vec2.add(from * a, to * b);
	}

	static nlerp = function (s, e, t) {
		let linear = new vec2(s.x + (e.x - s.x) * t, s.y + (e.y - s.y) * t);
		return _vec2_normalized(linear);
	}

	static compare = function (l, r) {
		let dx = l.x - r.x;
		let dy = l.y - r.y;
		let lenSq = dx * dx + dy * dy;
		return lenSq < vec2.epsilon;
	}

}

class vec3 {
	constructor(x, y, z) {
		this.x = (typeof x === "undefined")? 0.0 : x;
		this.y = (typeof y === "undefined")? 0.0 : y;
		this.z = (typeof z === "undefined")? 0.0 : z;
	}
	static epsilon = 0.0001;

	static add = function (left, right) {
		return new vec3(left.x + right.x, left.y + right.y, left.z + right.z);
	}

	static sub = function (left, right) {
		return new vec3(left.x - right.x, left.y - right.y, left.z - right.z);
	}

	static mul = function (left, right) {
		return new vec3(left.x * right.x, left.y * right.y, left.z * right.z);
	}

	static div = function (left, right) {
		return new vec3(left.x / right.x, left.y / right.y, left.z / right.z);
	}

	static scale = function (left, right) {
		return new vec3(left.x * right, left.y * right, left.z * right);
	}

	static dot = function (left, right) {
		return left.x * right.x + left.y * right.y + left.z * right.z;
	}

	static len = function (v) {
		let res = v.x * v.x + v.y * v.y + v.z * v.z;
		if (res < vec3.epsilon) {
			return 0.0;
		}
		return Math.sqrt(res);
	}

	static lenSq = function (v) {
		return v.x * v.x + v.y * v.y + v.z * v.z;
	}

	static normalized = function (v) {
		let res = v.x * v.x + v.y * v.y + v.z * v.z;
		if (res < vec3.epsilon) {
			return v;
		}
		let invLen = 1.0 / Math.sqrt(res);
		return new vec3(v.x * invLen, v.y * invLen, v.z * invLen);
	}

	static angle = function (l, r) {
		let sqMagL = l.x * l.x + l.y * l.y + l.z * l.z;
		let sqMagR = r.x * r.x + r.y * r.y + r.z * r.z;
		if (sqMagL < vec3.epsilon || sqMagR < vec3.epsilon) {
			return 0.0;
		}
		let dot = l.x * r.x + l.y * r.y + l.z * r.z;
		let len = Math.sqrt(sqMagL) * Math.sqrt(sqMagR);
		return Math.acos(dot / len);
	}

	static project = function (a, b) {
		let magBSq = _vec3_len(b);
		if (magBSq < vec3.epsilon) {
			return new vec3();
		}
		let scale = _vec3_dot(a, b) / magBSq;
		return b * scale;
	}

	static reject = function (a, b) {
		let projection = _vec3_project(a, b);
		return vec3.sub(a, projection);
	}

	static reflect = function (a, b) {
		let magBSq = _vec3_len(b);
		if (magBSq < vec3.epsilon) {
			return new vec3();
		}
		let scale = _vec3_dot(a, b) / magBSq;
		let proj2 = b * (scale * 2);
		return vec3.sub(a, proj2);
	}

	static cross = function (l, r) {
		return new vec3(l.y * r.z - l.z * r.y, l.z * r.x - l.x * r.z, l.x * r.y - l.y * r.x);
	}

	static lerp = function (s, e, t) {
		return new vec3(s.x + (e.x - s.x) * t, s.y + (e.y - s.y) * t, s.z + (e.z - s.z) * t);
	}

	static slerp = function (s, e, t) {
		if (t < vec3.epsilon) {
			return _vec3_lerp(s, e, t);
		}
		let from = _vec3_normalized(s);
		let to = _vec3_normalized(e);
		let theta = _vec3_angle(from, to);
		let sin_theta = Math.sin(theta);
		let a = Math.sin((1.0 - t) * theta) / sin_theta;
		let b = Math.sin(t * theta) / sin_theta;
		return vec3.add(from * a, to * b);
	}

	static nlerp = function (s, e, t) {
		let linear = new vec3(s.x + (e.x - s.x) * t, s.y + (e.y - s.y) * t, s.z + (e.z - s.z) * t);
		return _vec3_normalized(linear);
	}

	static compare = function (l, r) {
		let dx = l.x - r.x;
		let dy = l.y - r.y;
		let dz = l.z - r.z;
		let lenSq = dx * dx + dy * dy + dz * dz;
		return lenSq < vec3.epsilon;
	}

}

class vec4 {
	constructor(x, y, z, w) {
		this.x = (typeof x === "undefined")? 0.0 : x;
		this.y = (typeof y === "undefined")? 0.0 : y;
		this.z = (typeof z === "undefined")? 0.0 : z;
		this.w = (typeof w === "undefined")? 0.0 : w;
	}
	static epsilon = 0.0001;

	static add = function (left, right) {
		return new vec4(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);
	}

	static sub = function (left, right) {
		return new vec4(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);
	}

	static mul = function (left, right) {
		return new vec4(left.x * right.x, left.y * right.y, left.z * right.z, left.w * right.w);
	}

	static div = function (left, right) {
		return new vec4(left.x / right.x, left.y / right.y, left.z / right.z, left.w / right.w);
	}

	static scale = function (left, right) {
		return new vec4(left.x * right, left.y * right, left.z * right, left.w * right);
	}

	static dot = function (left, right) {
		return left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w;
	}

	static len = function (v) {
		let res = v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
		if (res < vec4.epsilon) {
			return 0.0;
		}
		return Math.sqrt(res);
	}

	static lenSq = function (v) {
		return v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
	}

	static normalized = function (v) {
		let res = v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
		if (res < vec4.epsilon) {
			return v;
		}
		let invLen = 1.0 / Math.sqrt(res);
		return new vec4(v.x * invLen, v.y * invLen, v.z * invLen, v.w * invLen);
	}

	static angle = function (l, r) {
		let sqMagL = l.x * l.x + l.y * l.y + l.z * l.z + l.w * l.w;
		let sqMagR = r.x * r.x + r.y * r.y + r.z * r.z + r.w * r.w;
		if (sqMagL < vec4.epsilon || sqMagR < vec4.epsilon) {
			return 0.0;
		}
		let dot = l.x * r.x + l.y * r.y + l.z * r.z * l.w * r.w;
		let len = Math.sqrt(sqMagL) * Math.sqrt(sqMagR);
		return Math.acos(dot / len);
	}

	static project = function (a, b) {
		let magBSq = _vec4_len(b);
		if (magBSq < vec4.epsilon) {
			return new vec4();
		}
		let scale = _vec4_dot(a, b) / magBSq;
		return b * scale;
	}

	static reject = function (a, b) {
		let projection = _vec4_project(a, b);
		return vec4.sub(a, projection);
	}

	static reflect = function (a, b) {
		let magBSq = _vec4_len(b);
		if (magBSq < vec4.epsilon) {
			return new vec4();
		}
		let scale = _vec4_dot(a, b) / magBSq;
		let proj2 = b * (scale * 2);
		return vec4.sub(a, proj2);
	}

	static cross = function (l, r) {
		return new vec3(l.y * r.z - l.z * r.y, l.z * r.x - l.x * r.z, l.x * r.y - l.y * r.x);
	}

	static lerp = function (s, e, t) {
		return new vec4(s.x + (e.x - s.x) * t, s.y + (e.y - s.y) * t, s.z + (e.z - s.z) * t, s.w + (e.w - s.w) * t);
	}

	static slerp = function (s, e, t) {
		if (t < vec4.epsilon) {
			return _vec4_lerp(s, e, t);
		}
		let from = _vec4_normalized(s);
		let to = _vec4_normalized(e);
		let theta = _vec4_angle(from, to);
		let sin_theta = Math.sin(theta);
		let a = Math.sin((1.0 - t) * theta) / sin_theta;
		let b = Math.sin(t * theta) / sin_theta;
		return vec4.add(from * a, to * b);
	}

	static nlerp = function (s, e, t) {
		let linear = new vec4(s.x + (e.x - s.x) * t, s.y + (e.y - s.y) * t, s.z + (e.z - s.z) * t, s.w + (e.w - s.w) * t);
		return _vec4_normalized(linear);
	}

	static compare = function (l, r) {
		let dx = l.x - r.x;
		let dy = l.y - r.y;
		let dz = l.z - r.z;
		let dw = l.w - r.w;
		let lenSq = dx * dx + dy * dy + dz * dz + dw * dw;
		return lenSq < vec4.epsilon;
	}

}

function main(args) {
	let right = new vec3(1, 0, 0);
	let up = new vec3(0, 1, 0);
	let forward = new vec3(0, 0, 1);
	let half = vec3.lerp(right, up, 0.5);
	console.log("right: " + ("(" + right.x + ", " + right.y + ", " + right.z + ")"));
	console.log("up: " + (("(" + up.x + ", " + up.y + ", " + up.z + ")")));
	console.log("forward: " + ("(" + forward.x + ", " + forward.y + ", " + forward.z + ")"));
	console.log("half: " + ("(" + half.x + ", " + half.y + ", " + half.z + ")"));
	console.log("dot: " + vec3.dot(right, up));
	let a = vec3.add(up, right);
	let b = vec3.sub(up, right);
	let c = vec3.mul(up, right);
	let e = vec3.div(up, right);
	let d = up * 0.5;
	let same = vec3.compare(up, right);
	let thisTime = vec3.compare(up, new vec3(0, 1, 0));
	let hammerTime = !vec3.compare(up, new vec3(0, 1, 0));
	if (same) {
		console.log("wrong");
	}
	else if (thisTime) {
		console.log("right!");
	}
	else {
		console.log("wrong");
	}
	return 0;
}

