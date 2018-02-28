namespace Kiotlog {

    const {customElement, property} = Polymer.decorators;

    @customElement('ktlg-devices')
    export class KtlgDevices extends Polymer.Element {
        @property({type: String})
        apiUrl: string; // TODO: should set a default value?
    }
}