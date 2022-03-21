function waitForPrism() {
   if (typeof Prism !== 'undefined') {
       var el = document.getElementById("post")
       console.log('hightlighting code in', el)
       Prism.highlightAllUnder(el)
   } else {
       setTimeout(waitForPrism, 250) 
   }
}
waitForPrism()
