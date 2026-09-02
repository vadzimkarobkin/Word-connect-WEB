const aspectRatio = (function () {

	const aspectRatioDesktop = "9/16";
	let gameCanvas = document.querySelector("#unity-canvas");
	let aspectRatio = -1; // По умолчанию не блокируем

	function parseAspectRatio(input) {
		let fractionParts = input.split('/');
		if (fractionParts.length === 2) {
			const numerator = parseFloat(fractionParts[0]);
			const denominator = parseFloat(fractionParts[1]);
			if (!isNaN(numerator) && !isNaN(denominator) && denominator !== 0) {
				return numerator / denominator;
			}
		}
		return -1;
	}

	function centerAlignCanvas() {
		gameCanvas.style.margin = "auto";
		gameCanvas.style.top = "0";
		gameCanvas.style.left = "0";
		gameCanvas.style.bottom = "0";
		gameCanvas.style.right = "0";
	}

	function recalculateAspectRatio() {
		let windowWidth = window.innerWidth;
		let windowHeight = window.innerHeight;

		if (windowWidth / windowHeight > aspectRatio) {
			gameCanvas.style.width = Math.floor(windowHeight * aspectRatio) + "px";
			gameCanvas.style.height = "100%";
		} else {
			gameCanvas.style.width = "100%";
			gameCanvas.style.height = Math.floor(windowWidth / aspectRatio) + "px";
		}

		centerAlignCanvas();
	}

	function resetAspectRatio() {
		gameCanvas.style.width = "100%";
		gameCanvas.style.height = "100%";
		centerAlignCanvas();
	}

	function selectAspectRatio() {
		resetAspectRatio();
		if (aspectRatio > 0) {
			recalculateAspectRatio();
		}
	}

	return {
		init: function () {
			// Если десктоп — задаем фиксированное соотношение
			if (!/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
				aspectRatio = parseAspectRatio(aspectRatioDesktop);
			}

			window.addEventListener("load", selectAspectRatio);
			window.addEventListener("resize", selectAspectRatio);
			document.addEventListener("readystatechange", selectAspectRatio);
			document.addEventListener("DOMContentLoaded", selectAspectRatio);
		}
	};

})();
