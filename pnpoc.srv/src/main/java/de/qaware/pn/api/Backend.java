package de.qaware.pn.api;

import com.microsoft.azure.sdk.iot.service.exceptions.IotHubException;
import com.microsoft.azure.storage.StorageException;
import de.qaware.pn.service.IotHubConnection;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import javax.ws.rs.Consumes;
import javax.ws.rs.QueryParam;
import java.io.IOException;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;

@RestController
public class Backend {
    private IotHubConnection iotHubConnection;

    @Autowired
    public Backend(IotHubConnection iotHubConnection) {
        this.iotHubConnection = iotHubConnection;
    }

    @RequestMapping(method = RequestMethod.POST, path = "message")
    @Consumes("application/json")
    public String sentPushMessage(@RequestBody String deviceId) {
        System.out.println("sending " + deviceId);
        Object resultPayload = null;
        try {
            iotHubConnection.sendMessageToDevice(deviceId);
//            resultPayload = iotHubConnection.callMethod("device1234","testMethod", "refresh your data");
        } catch (IOException | InterruptedException | IotHubException e) {
            e.printStackTrace();
        }

        return "ok";
    }

    @RequestMapping("sas")
    public String getSas(@QueryParam("deviceId") String deviceId) {
        try {
            return iotHubConnection.getContainerSasUri(null);
        } catch (InvalidKeyException | StorageException e) {
            e.printStackTrace();
        }
        return null;
    }

    @RequestMapping(method = RequestMethod.PUT, path = "device")
    public void registerDevice(@RequestBody String deviceId) {
        try {
            iotHubConnection.registerDevice(deviceId);
        } catch (IOException | NoSuchAlgorithmException e) {
            e.printStackTrace();
        }
    }

}
