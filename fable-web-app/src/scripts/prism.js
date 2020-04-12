import prism from 'prismjs'
import 'prismjs/components/prism-fsharp'
import 'prismjs/components/prism-python'
import 'prismjs/components/prism-bash'
import '../styles/prism.css'

const highlightAllUnder = ref => {
    prism.highlightAllUnder(ref)
}

export default highlightAllUnder
