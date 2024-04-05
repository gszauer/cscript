var assert = null;

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

class quat {
	constructor(x, y, z, w) {
		this.x = (typeof x === "undefined")? 0.0 : x;
		this.y = (typeof y === "undefined")? 0.0 : y;
		this.z = (typeof z === "undefined")? 0.0 : z;
		this.w = (typeof w === "undefined")? 1.0 : w;
	}
	static epsilon = 0.0001;

	static fromAngleAxis = function (angle, axis) {
		let norm = vec3.normalized(axis);
		let s = Math.sin(angle * 0.5);
		return new quat(norm.x * s, norm.y * s, norm.z * s, Math.cos(angle * 0.5));
	}

	static fromTo = function (from, to) {
		let f = vec3.normalized(from);
		let t = vec3.normalized(to);
		if (vec3.compare(f, t)) {
			return new quat();
		}
		else if (vec3.compare(f, t * -1.0)) {
			let ortho = new vec3(1, 0, 0);
			if (Math.abs(f.y) < Math.abs(f.x)) {
				ortho = new vec3(0, 1, 0);
			}
			if (Math.abs(f.z) < Math.abs(f.y) && Math.abs(f.z) < Math.abs(f.x)) {
				ortho = new vec3(0, 0, 1);
			}
			let axis = vec3.normalized(vec3.cross(f, ortho));
			return new quat(axis.x, axis.y, axis.z, 0);
		}
		let half = vec3.normalized(vec3.add(f, t));
		let axis = vec3.cross(f, half);
		return new quat(axis.x, axis.y, axis.z, vec3.dot(f, half));
	}

	static look = function (direcion, up) {
		let f = vec3.normalized(direcion);
		let u = vec3.normalized(up);
		let r = vec3.cross(u, f);
		u = vec3.cross(f, r);
		let _up = new vec3(0, 1, 0);
		let _forward = new vec3(0, 0, 1);
		let f2d = quat.fromTo(_forward, f);
		let objectUp = f2d * _up;
		let u2u = quat.fromTo(objectUp, u);
		let result = quat.mul(f2d, u2u);
		return quat.normalized(result);
	}

	static getAxis = function (quat) {
		return vec3.normalized(new vec3(quat.x, quat.y, quat.z));
	}

	static getAngle = function (quat) {
		return 2.0 * Math.acos(quat.w);
	}

	static add = function (a, b) {
		return new quat(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	}

	static sub = function (a, b) {
		return new quat(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	}

	static mul = function (Q1, Q2) {
		return new quat(Q2.x * Q1.w + Q2.y * Q1.z - Q2.z * Q1.y + Q2.w * Q1.x, -Q2.x * Q1.z + Q2.y * Q1.w + Q2.z * Q1.x + Q2.w * Q1.y, Q2.x * Q1.y - Q2.y * Q1.x + Q2.z * Q1.w + Q2.w * Q1.z, -Q2.x * Q1.x - Q2.y * Q1.y - Q2.z * Q1.z + Q2.w * Q1.w);
	}

	static rotate = function (q, v) {
		let vector = new vec3(q.x, q.y, q.z);
		let scalar = q.w;
		return vec3.add(vec3.add(vector * 2.0 * vec3.dot(vector, v), v * (scalar * scalar - vec3.dot(vector, vector))), vec3.cross(vector, v) * 2.0 * scalar);
	}

	static scale = function (a, b) {
		return new quat(a.x * b, a.y * b, a.z * b, a.w * b);
	}

	static negate = function (q) {
		return new quat(-q.x, -q.y, -q.z, -q.w);
	}

	static compare = function (left, right) {
		return (Math.abs(left.x - right.x) <= quat.epsilon && Math.abs(left.y - right.y) <= quat.epsilon && Math.abs(left.z - right.z) <= quat.epsilon && Math.abs(left.w - right.w) <= quat.epsilon);
	}

	static compareOrientation = function (left, right) {
		return (Math.abs(left.x - right.x) <= quat.epsilon && Math.abs(left.y - right.y) <= quat.epsilon && Math.abs(left.z - right.z) <= quat.epsilon && Math.abs(left.w - right.w) <= quat.epsilon) || (Math.abs(left.x + right.x) <= quat.epsilon && Math.abs(left.y + right.y) <= quat.epsilon && Math.abs(left.z + right.z) <= quat.epsilon && Math.abs(left.w + right.w) <= quat.epsilon);
	}

	static dot = function (a, b) {
		return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}

	static lenSq = function (q) {
		return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
	}

	static len = function (q) {
		let lenSq = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
		if (lenSq < quat.epsilon) {
			return 0.0;
		}
		return Math.sqrt(lenSq);
	}

	static normalized = function (q) {
		let lenSq = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
		if (lenSq < quat.epsilon) {
			return new quat();
		}
		let i_len = 1.0 / Math.sqrt(lenSq);
		return new quat(q.x * i_len, q.y * i_len, q.z * i_len, q.w * i_len);
	}

	static conjugate = function (q) {
		return new quat(-q.x, -q.y, -q.z, q.w);
	}

	static inverse = function (q) {
		let lenSq = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
		if (lenSq < quat.epsilon) {
			return new quat();
		}
		let recip = 1.0 / lenSq;
		return new quat(-q.x * recip, -q.y * recip, -q.z * recip, q.w * recip);
	}

	static mix = function (from, to, t) {
		return quat.add(from * (1.0 - t), to * t);
	}

	static nlerp = function (from, to, t) {
		return quat.normalized(quat.add(from, (quat.sub(to, from)) * t));
	}

	static slerp = function (start, end, t) {
		if (Math.abs(quat.dot(start, end)) > 1.0 - quat.epsilon) {
			return quat.nlerp(start, end, t);
		}
		return quat.normalized(quat.mul((quat.pow(quat.mul(quat.inverse(start), end), t)), start));
	}

	static pow = function (q, f) {
		let vector = new vec3(q.x, q.y, q.z);
		let scalar = q.w;
		let angle = 2.0 * Math.acos(scalar);
		let axis = vec3.normalized(vector);
		let halfCos = Math.cos(f * angle * 0.5);
		let halfSin = Math.sin(f * angle * 0.5);
		return new quat(axis.x * halfSin, axis.y * halfSin, axis.z * halfSin, halfCos);
	}

}

function main(args) {
	let four = Math.sqrt(16) * -1.0;
	four = Math.abs(four);
	console.log("Four: " + four);
	let display = "Hello, world";
	let firstO = display.indexOf('o');
	let lastO = display.lastIndexOf("o");
	let comma = display.indexOf(",");
	console.log("" + display.at(firstO) + display[comma] + display[lastO]);
	if (display.startsWith('H')) {
		console.log("true 1");
	}
	if (display.startsWith("Hel")) {
		console.log("true 2");
	}
	if (!display.startsWith("He3")) {
		console.log("true 3");
	}
	display = "hello my love";
	let exploded = display.split(" ");
	display = exploded.join("-");
	console.log(display);
	let x1 = "hello";
	let x2 = " world";
	display = x1.concat(x2);
	console.log(display);
	display = display.toUpperCase();
	console.log(display);
	display = display.toLowerCase();
	console.log(display);
	display = "hello my love";
	let lova = display.substring(9, (4) + (9) );
	console.log(lova);
	lova = display.replace("hello", "hola");
	console.log(lova);
	return 0;
}

