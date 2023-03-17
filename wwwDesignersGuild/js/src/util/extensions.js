/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Gracias: https://stackoverflow.com/questions/78374
//
// Warn if overriding existing method
if (Array.prototype.equals)
    console.warn("Overriding existing Array.prototype.equals. Possible causes: New API defines the method, there's a framework conflict or you've got double inclusions in your code.");
// attach the .equals method to Array's prototype to call it on any array
Array.prototype.equals = function (array) {
    // if the other array is a falsy value, return
    if (!array)
        return false;

    // compare lengths - can save a lot of time 
    if (this.length != array.length)
        return false;

    for (var i = 0, l = this.length; i < l; i++) {
        // Check if we have nested arrays
        if (this[i] instanceof Array && array[i] instanceof Array) {
            // recurse into the nested arrays
            if (!this[i].equals(array[i]))
                return false;
        }
        else if (this[i] != array[i]) {
            // Warning - two different object instances will never be equal: {x:20} != {x:20}
            return false;
        }
    }
    return true;
}
// Hide method from for-in loops
Object.defineProperty(Array.prototype, "equals", { enumerable: false });
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Adapted from: https://stackoverflow.com/questions/42604185/get-the-intersection-of-n-arrays#answer-42604856
// With thanks
//
// Warn if overriding existing method
if (Set.prototype.multipleIntersect)
    console.warn("Overriding existing Set.prototype.multipleIntersect. Possible causes: New API defines the method, there's a framework conflict or you've got double inclusions in your code.");
// attach the .multipleIntersect method to Set's prototype to call it on any set
Set.prototype.multipleIntersect = function (...sets) {
    // if the other array is a falsy value, return
    let count = sets.length;
    if (sets.length) {
        this.forEach(item => {
            let i = count;
            let allHave = true;
            while (i--) {
                allHave = sets[i].has(item);
                if (!allHave)
                    break;
            }
            if (!allHave)
                this.delete(item);
        });
    }
    return this;
}
// Hide method from for-in loops
Object.defineProperty(Set.prototype, "multipleIntersect", { enumerable: false });
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
