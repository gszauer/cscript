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

