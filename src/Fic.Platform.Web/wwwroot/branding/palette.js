window.ficBranding = window.ficBranding || {};

window.ficBranding.extractPaletteFromDataUrl = async function (dataUrl) {
    const image = await loadImage(dataUrl);
    const maxDimension = 64;
    const scale = Math.min(1, maxDimension / Math.max(image.width, image.height));
    const width = Math.max(1, Math.round(image.width * scale));
    const height = Math.max(1, Math.round(image.height * scale));

    const canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;

    const context = canvas.getContext("2d", { willReadFrequently: true });
    context.drawImage(image, 0, 0, width, height);

    const imageData = context.getImageData(0, 0, width, height).data;
    const buckets = new Map();

    for (let index = 0; index < imageData.length; index += 4) {
        const alpha = imageData[index + 3];
        if (alpha < 140) {
            continue;
        }

        const red = imageData[index];
        const green = imageData[index + 1];
        const blue = imageData[index + 2];
        const { saturation, lightness } = rgbToHsl(red, green, blue);

        if (lightness < 0.05 || lightness > 0.96) {
            continue;
        }

        const bucketRed = quantize(red);
        const bucketGreen = quantize(green);
        const bucketBlue = quantize(blue);
        const key = `${bucketRed},${bucketGreen},${bucketBlue}`;
        const weight = 1 + (saturation * 2.8) + Math.abs(0.5 - lightness);

        if (!buckets.has(key)) {
            buckets.set(key, { red: bucketRed, green: bucketGreen, blue: bucketBlue, score: 0, saturation, lightness });
        }

        const bucket = buckets.get(key);
        bucket.score += weight;
    }

    const colors = [...buckets.values()].sort((left, right) => right.score - left.score);
    if (colors.length === 0) {
        return ["#1f3731", "#f4c15d"];
    }

    const primary = colors.find((entry) => entry.saturation >= 0.16) || colors[0];
    const accent = pickAccent(colors, primary);

    return [toHex(primary), toHex(accent)];
};

function quantize(value) {
    return Math.max(0, Math.min(255, Math.round(value / 24) * 24));
}

function pickAccent(colors, primary) {
    let best = null;
    let bestScore = -1;

    for (const candidate of colors) {
        const distance = colorDistance(candidate, primary);
        const score = (candidate.score * 0.8) + (candidate.saturation * 120) + distance;

        if (distance < 72) {
            continue;
        }

        if (score > bestScore) {
            best = candidate;
            bestScore = score;
        }
    }

    if (best) {
        return best;
    }

    const lighten = primary.lightness < 0.45 ? 0.22 : -0.2;
    return adjustLightness(primary, lighten);
}

function adjustLightness(color, delta) {
    const { hue, saturation, lightness } = rgbToHsl(color.red, color.green, color.blue);
    return hslToRgb(hue, Math.max(saturation, 0.24), clamp(lightness + delta, 0.08, 0.88));
}

function colorDistance(left, right) {
    const red = left.red - right.red;
    const green = left.green - right.green;
    const blue = left.blue - right.blue;
    return Math.sqrt((red * red) + (green * green) + (blue * blue));
}

function toHex(color) {
    return `#${channelToHex(color.red)}${channelToHex(color.green)}${channelToHex(color.blue)}`;
}

function channelToHex(value) {
    return value.toString(16).padStart(2, "0");
}

function clamp(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

function rgbToHsl(red, green, blue) {
    const r = red / 255;
    const g = green / 255;
    const b = blue / 255;
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const lightness = (max + min) / 2;

    if (max === min) {
        return { hue: 0, saturation: 0, lightness };
    }

    const delta = max - min;
    const saturation = lightness > 0.5
        ? delta / (2 - max - min)
        : delta / (max + min);

    let hue;
    switch (max) {
        case r:
            hue = ((g - b) / delta) + (g < b ? 6 : 0);
            break;
        case g:
            hue = ((b - r) / delta) + 2;
            break;
        default:
            hue = ((r - g) / delta) + 4;
            break;
    }

    hue /= 6;
    return { hue, saturation, lightness };
}

function hslToRgb(hue, saturation, lightness) {
    if (saturation === 0) {
        const value = Math.round(lightness * 255);
        return { red: value, green: value, blue: value };
    }

    const q = lightness < 0.5
        ? lightness * (1 + saturation)
        : lightness + saturation - (lightness * saturation);
    const p = (2 * lightness) - q;

    const red = hueToRgb(p, q, hue + (1 / 3));
    const green = hueToRgb(p, q, hue);
    const blue = hueToRgb(p, q, hue - (1 / 3));

    return {
        red: Math.round(red * 255),
        green: Math.round(green * 255),
        blue: Math.round(blue * 255)
    };
}

function hueToRgb(p, q, t) {
    let hue = t;

    if (hue < 0) {
        hue += 1;
    }

    if (hue > 1) {
        hue -= 1;
    }

    if (hue < 1 / 6) {
        return p + ((q - p) * 6 * hue);
    }

    if (hue < 1 / 2) {
        return q;
    }

    if (hue < 2 / 3) {
        return p + ((q - p) * ((2 / 3) - hue) * 6);
    }

    return p;
}

function loadImage(dataUrl) {
    return new Promise((resolve, reject) => {
        const image = new Image();
        image.onload = () => resolve(image);
        image.onerror = () => reject(new Error("Could not load logo image for palette extraction."));
        image.src = dataUrl;
    });
}
