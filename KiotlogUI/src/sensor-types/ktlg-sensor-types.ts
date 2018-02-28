namespace Kiotlog {

    const {customElement, property} = Polymer.decorators;

    @customElement('ktlg-sensor-types')
    export class KtlgSensorTypes extends Polymer.Element {
        @property({type: String})
        apiUrl: string; // TODO: should set a default value?
    }
}