namespace Kiotlog {

    const {customElement, property} = Polymer.decorators;

    @customElement('ktlg-view404')
    class KtlgView404 extends Polymer.Element {
        // static get is() { return 'ktlg-view404'; }

        // This shouldn't be neccessary, but the Analyzer isn't picking up
        // Polymer.Element#rootPath
        @property({type: String})
        rootPath: string;
    }
}